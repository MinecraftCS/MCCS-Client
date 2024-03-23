using MineCS.mccs.level.tile;
using MineCS.mccs.phys;

namespace MineCS.mccs.level
{
    public class Level
    {
        public const int TILE_UPDATE_INTERVAL = 200;

        public int width;
        public int height;
        public int depth;
        public byte[] blockArray;
        private int[] lightDepths;
        public List<LevelRenderer> levelListeners = new List<LevelRenderer>();
        private Random random = new Random();
        public int tickCheck;
        public string name;
        public string username;
        public long creationTime;
        public int tickSpeed = 0;

        public void load(int width, int depth, int height, byte[] blockArray)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;
            this.blockArray = blockArray;
            lightDepths = new int[width * height];
            loadWorld(0, 0, width, height);
            for (int i = 0; i < levelListeners.Count; i++)
                levelListeners[i].loadRenderer();
        }

        private void loadWorld(int w0, int h0, int w1, int h1)
        {
            for (int x = w0; x < w0 + w1; x++)
            {
                for (int z = h0; z < h0 + h1; z++)
                {
                    int bottom = lightDepths[x + z * width];
                    int y = depth - 1;
                    while (y > 0)
                    {
                        Tile tile = Tile.tiles[getTile(x, y, z)];
                        if (tile != null && tile.blocksLight()) break;
                        y--;
                    }
                    lightDepths[x + z * width] = y + 1;
                    if (bottom == y) continue;
                    int top = bottom < y ? bottom : y;
                    bottom = bottom > y ? bottom : y;
                    for (int i = 0; i < levelListeners.Count; i++)
                        levelListeners[i].setDirty(x - 1, top - 1, z - 1, x + 1, bottom + 1, z + 1);
                }
            }
        }

        public List<AABB> getCubes(AABB bb)
        {
            List<AABB> surroundings = new List<AABB>();
            int xf0 = (int)Math.Floor(bb.x0);
            int xf1 = (int)Math.Floor(bb.x1 + 1.0f);
            int yf0 = (int)Math.Floor(bb.y0);
            int yf1 = (int)Math.Floor(bb.y1 + 1.0f);
            int zf0 = (int)Math.Floor(bb.z0);
            int zf1 = (int)Math.Floor(bb.z1 + 1.0f);
            for (int x = xf0; x < xf1; x++)
                for (int y = yf0; y < yf1; y++)
                    for (int z = zf0; z < zf1; z++)
                    {
                        if (x >= 0 && y >= 0 && z >= 0 && x < width && y < depth && z < height)
                        {
                            Tile tile = Tile.tiles[getTile(x, y, z)];
                            if (tile != null)
                            {
                                AABB tileAABB = tile.getAABB(x, y, z);
                                if (tileAABB != null)
                                    surroundings.Add(tileAABB);
                            }
                        }
                        AABB bedrockAABB = Tile.bedrock.getAABB(x, y, z);
                        if ((x < 0 || y < 0 || z < 0 || x >= width || z >= height) && bedrockAABB != null)
                            surroundings.Add(bedrockAABB);
                    }
            return surroundings;
        }

        public bool setTile(int x, int y, int z, int type)
        {
            if (x < 0 || y < 0 || z < 0 || x >= width || y >= depth || z >= height)
                return false;
            if (type == blockArray[(y * height + z) * width + x])
                return false;
            blockArray[(y * height + z) * width + x] = (byte)type;
            updateFaces(x - 1, y, z, type);
            updateFaces(x + 1, y, z, type);
            updateFaces(x, y - 1, z, type);
            updateFaces(x, y + 1, z, type);
            updateFaces(x, y, z - 1, type);
            updateFaces(x, y, z + 1, type);
            loadWorld(x, z, 1, 1);
            for (int i = 0; i < levelListeners.Count; i++)
                levelListeners[i].setDirty(x - 1, y - 1, z - 1, x + 1, y + 1, z + 1);
            return true;
        }

        public bool tryReplaceTile(int x, int y, int z, int id)
        {
            if (x < 0 || y < 0 || z < 0 || x >= width || y >= depth || z >= height)
                return false;
            if (id == blockArray[(y * height + z) * width + x])
                return false;
            blockArray[(y * height + z) * width + x] = (byte)id;
            return true;
        }

        private void updateFaces(int x, int y, int z, int type)
        {
            if (x < 0 || y < 0 || z < 0 || x >= width || y >= depth || z >= height)
                return;
            Tile b2 = Tile.tiles[blockArray[(y * height + z) * width + x]];
            if (b2 != null)
                b2.checkFaces(this, x, y, z, type);
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
            return blockArray[(y * height + z) * width + x];
        }

        public bool isTileInRange(AABB aabb, int type)
        {
            int x0 = (int)Math.Floor(aabb.x0);
            int x1 = (int)Math.Floor(aabb.x1 + 1.0f);
            int y0 = (int)Math.Floor(aabb.y0);
            int y1 = (int)Math.Floor(aabb.y1 + 1.0f);
            int z0 = (int)Math.Floor(aabb.z0);
            int z1 = (int)Math.Floor(aabb.z1 + 1.0f);
            if (x0 < 0) x0 = 0;
            if (y0 < 0) y0 = 0;
            if (z0 < 0) z0 = 0;
            if (x1 > width) x1 = width;
            if (y1 > depth) y1 = depth;
            if (z1 > height) z1 = height;
            for (; x0 < x1; x0++)
                for (; y0 < y1; y0++)
                    for (; z0 < z1; z0++)
                    {
                        Tile b2 = Tile.tiles[getTile(x0, y0, z0)];
                        if (b2 != null && b2.getType() == type)
                            return true;
                    }
            return false;
        }

        public void tick()
        {
            tickSpeed += width * height * depth;
            int ticks = tickSpeed / TILE_UPDATE_INTERVAL;
            tickSpeed -= ticks * TILE_UPDATE_INTERVAL;
            for (int i = 0; i < ticks; i++)
            {
                tickCheck = tickCheck * 1664525 + 1013904223;
                int x = tickCheck >> 16 & width - 1;
                tickCheck = tickCheck * 1664525 + 1013904223;
                int y = tickCheck >> 16 & depth - 1;
                tickCheck = tickCheck * 1664525 + 1013904223;
                int z = tickCheck >> 16 & height - 1;
                int tile = getTile(x, y, z);
                if (Tile.doTick[tile])
                    Tile.tiles[tile].tick(this, x, y, z, random);
            }
        }
    }
}
