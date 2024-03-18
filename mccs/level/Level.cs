using MineCS.mccs.level.tile;
using MineCS.mccs.phys;
using System;
using System.IO.Compression;

namespace MineCS.mccs.level
{
    public class Level
    {
        public int width;
        public int height;
        public int depth;
        private byte[] blocks;
        private int[] lightDepths;
        private List<LevelListener> levelListeners = new List<LevelListener>();
        private Random random = new Random();
        int unprocessed = 0;

        public Level(int w, int h, int d)
        {
            width = w;
            height = h;
            depth = d;
            blocks = new byte[w * h * d];
            lightDepths = new int[w * h];
            bool mapLoaded = load();
            if (!mapLoaded)
                generateMap();
            calcLightDepths(0, 0, w, h);
        }

        private void generateMap()
        {
            int w = width;
            int h = height;
            int d = depth;
            int[] heightmap1 = new PerlinNoiseFilter(0).read(w, h);
            int[] heightmap2 = new PerlinNoiseFilter(0).read(w, h);
            int[] cf = new PerlinNoiseFilter(1).read(w, h);
            int[] rockmap = new PerlinNoiseFilter(1).read(w, h);
            for (int x = 0; x < w; x++)
                for (int y = 0; y < d; y++)
                    for (int z = 0; z < h; z++)
                    {
                        int dh;
                        int dh1 = heightmap1[x + z * width];
                        int dh2 = heightmap2[x + z * width];
                        int cfh = cf[x + z * width];
                        if (cfh < 128)
                            dh2 = dh1;
                        if (dh2 > (dh = dh1))
                            dh = dh2;
                        else
                            dh2 = dh1;
                        dh = dh / 8 + d / 3;
                        int rh = rockmap[x + z * width] / 8 + d / 3;
                        if (rh > dh - 2)
                            rh = dh - 2;
                        int i = (y * height + z) * width + x;
                        int id = 0;
                        if (y == dh)
                            id = Tile.grass.id;
                        if (y < dh)
                            id = Tile.dirt.id;
                        if (y <= rh)
                            id = Tile.rock.id;
                        blocks[i] = (byte)id;
                    }
        }

        public bool load()
        {
            using (var gz = new GZipStream(new FileStream("level.dat", FileMode.OpenOrCreate), CompressionMode.Decompress))
            {
                using (var ms = new MemoryStream())
                {
                    gz.CopyTo(ms);
                    if (ms.Length == 0)
                        return false;
                    blocks = ms.ToArray();
                }
            }
            calcLightDepths(0, 0, width, height);
            for (int i = 0; i < levelListeners.Count; i++)
                levelListeners[i].allChanged();
            return true;
        }

        public void save()
        {
            GZipStream dos = new GZipStream(new FileStream("level.dat", FileMode.Create), CompressionLevel.Optimal);
            dos.Write(blocks);
            dos.Flush();
            dos.Dispose();
        }

        public void calcLightDepths(int x0, int y0, int x1, int y1)
        {
            for (int x = x0; x < x0 + x1; x++)
            {
                for (int z = y0; z < y0 + y1; z++)
                {
                    int oldDepth = lightDepths[x + z * width];
                    int y = depth - 1;
                    while (y > 0 && !isLightBlocker(x, y, z))
                        y--;
                    lightDepths[x + z * width] = y;
                    if (oldDepth != y)
                    {
                        int yl0 = (oldDepth < y) ? oldDepth : y;
                        int yl1 = (oldDepth > y) ? oldDepth : y;
                        for (int i = 0; i < levelListeners.Count; i++)
                            levelListeners[i].lightColumnChanged(x, z, yl0, yl1);
                    }
                }
            }
        }

        public void addListener(LevelListener levelListener)
        {
            levelListeners.Add(levelListener);
        }

        public void removeListener(LevelListener levelListener)
        {
            levelListeners.Remove(levelListener);
        }

        public bool isLightBlocker(int x, int y, int z)
        {
            Tile tile = Tile.tiles[getTile(x, y, z)];
            return tile != null && tile.blocksLight();
        }

        public List<AABB> getCubes(AABB aABB)
        {
            List<AABB> aABBs = new List<AABB>();
            int x0 = (int)aABB.x0;
            int x1 = (int)(aABB.x1 + 1.0f);
            int y0 = (int)aABB.y0;
            int y1 = (int)(aABB.y1 + 1.0f);
            int z0 = (int)aABB.z0;
            int z1 = (int)(aABB.z1 + 1.0f);
            if (x0 < 0)
                x0 = 0;
            if (y0 < 0)
                y0 = 0;
            if (z0 < 0)
                z0 = 0;
            if (x1 > width)
                x1 = width;
            if (y1 > depth)
                y1 = depth;
            if (z1 > height)
                z1 = height;
            for (int x = x0; x < x1; x++)
                for (int y = y0; y < y1; y++)
                    for (int z = z0; z < z1; z++)
                    {
                        Tile tile = Tile.tiles[getTile(x, y, z)];
                        AABB? aabb = tile?.getAABB(x, y, z);
                        if (tile != null && aabb != null)
                            aABBs.Add(aabb);
                    }
            return aABBs;
        }

        public bool setTile(int x, int y, int z, int type)
        {
            if (x < 0 || y < 0 || z < 0 || x >= width || y >= depth || z >= height)
                return false;
            if (type == blocks[(y * height + z) * width + x])
                return false;
            blocks[(y * height + z) * width + x] = (byte)type;
            calcLightDepths(x, z, 1, 1);
            for (int i = 0; i < levelListeners.Count; i++)
                levelListeners[i].tileChanged(x, y, z);
            return true;
        }

        public bool isLit(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= width || y >= depth || z >= height)
                return true;
            return y >= lightDepths[x + z * width];
        }

        public int getTile(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= width || y >= depth || z >= height)
                return 0;
            return blocks[(y * height + z) * width + x];
        }

        public bool isSolidTile(int x, int y, int z)
        {
            Tile tile = Tile.tiles[getTile(x, y, z)];
            if (tile == null) return false;
            return tile.isSolid();
        }

        public void tick()
        {
            unprocessed += width * height * depth;
            int ticks = unprocessed / 400;
            unprocessed -= ticks * 400;
            for (int i = 0; i < ticks; i++)
            {
                int z = random.Next(height);
                int y = random.Next(depth);
                int x = random.Next(width);
                Tile tile = Tile.tiles[getTile(x, y, z)];
                if (tile != null)
                    tile.tick(this, x, y, z, random);
            }
        }
    }
}
