using MineCS.mccs.level.tile;
using MineCS.mccs.phys;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using System.IO.Compression;

namespace MineCS.mccs.level
{
    public class Level
    {
        public const int TILE_UPDATE_INTERVAL = 200;

        public int width;
        public int height;
        public int depth;
        private byte[] blockArray;
        private int[] lightDepths;
        public List<LevelRenderer> levelListeners = new List<LevelRenderer>();
        private Random random = new Random();
        public int tickCheck;
        private Client client;
        public int tickSpeed = 0;
        private int[] fluidLevels = new int[0x100000];

        public Level(Client client, int w, int h, int d)
        {
            this.client = client;
            w = client.width * 240 / client.height;
            h = client.height * 240 / client.height;
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0.0, w, h, 0.0, 100.0, 300.0);
            GL.MatrixMode(MatrixMode.Modelview0Ext);
            GL.LoadIdentity();
            GL.Translate(0.0f, 0.0f, -200.0f);
            client.loadingScreen("Loading level", "Reading..");
            width = 256;
            height = 256;
            depth = 64;
            blockArray = new byte[256 << 8 << 6];
            lightDepths = new int[256 << 8];
            bool mapLoaded = loadWorld();
            if (!mapLoaded)
                createLevel();
        }

        public void createLevel()
        {
            client.loadingScreen("Generating level", "Raising..");
            NoiseParams noiseParams = new NoiseParams(width, height, depth);
            int[] heightmap1 = new NoiseMap(noiseParams.random, 1, true ).read(width, height);
            int[] heightmap2 = new NoiseMap(noiseParams.random, 0, true ).read(width, height);
            int[] heightmap3 = new NoiseMap(noiseParams.random, 2, false).read(width, height);
            int[] heightmap4 = new NoiseMap(noiseParams.random, 4, false).read(width, height);
            int[] heightmap5 = new NoiseMap(noiseParams.random, 1, true ).read(width, height);
            blockArray = new byte[width * height * depth];
            int hDepth = depth / 2;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < depth; y++)
                    for (int z = 0; z < height; z++)
                    {
                        int h1 = heightmap1[x + z * width];
                        int h2 = heightmap2[x + z * width];
                        int h3 = heightmap3[x + z * width];
                        int h4 = heightmap4[x + z * width];

                        if (h3 < 128)
                            h2 = h1;

                        int grassLayer = h1;
                        if (h2 > grassLayer)
                            grassLayer = h2;

                        grassLayer = ((grassLayer - 128) / 8) + hDepth - 1;
                        int stoneLayer = ((heightmap5[x + z * width] - 128) / 6 + hDepth + grassLayer) / 2;
                        
                        if (h4 < 92)
                            grassLayer = grassLayer / 2 * 2;
                        else if (h4 < 160)
                            grassLayer = grassLayer / 4 * 4;

                        if (grassLayer < hDepth - 2)
                            grassLayer = (grassLayer - hDepth) / 2 + hDepth;
                        if (stoneLayer > grassLayer - 2)
                            stoneLayer = grassLayer - 2;

                        int tilePos = (y * height + z) * width + x;
                        int tileId = 0;

                        if (y == grassLayer && y >= hDepth)
                            tileId = Tile.grass.id;
                        if (y < grassLayer)
                            tileId = Tile.dirt.id;
                        if (y <= stoneLayer)
                            tileId = Tile.stone.id;
                        blockArray[tilePos] = (byte)tileId;
                    }
            client.loadingScreen("Generating level", "Carving..");
            int count = width * height * depth / 256 / 64;
            for (int i = 0; i < count; i++)
            {
                float x = random.NextSingle() * width;
                float y = random.NextSingle() * depth;
                float z = random.NextSingle() * height;
                int length = (int)(random.NextSingle() + random.NextSingle() * 150.0f);
                float dir1 = (float)(random.NextSingle() * Math.PI * 2.0);
                float dira1 = 0.0f;
                float dir2 = (float)(random.NextSingle() * Math.PI * 2.0);
                float dira2 = 0.0f;
                for (int la = 0; la < length; la++)
                {
                    x = (float)(x + Math.Sin(dir1) * Math.Cos(dir2));
                    z = (float)(z + Math.Cos(dir1) * Math.Cos(dir2));
                    y = (float)(y + Math.Sin(dir2));
                    dir1 += dira1 * 0.2f;
                    dira1 *= 0.9f;
                    dira1 += random.NextSingle() - random.NextSingle();
                    dir2 += dira2 * 0.5f;
                    dir2 *= 0.5f;
                    dira2 *= 0.9f;
                    dira2 += random.NextSingle() - random.NextSingle();
                    float size = (float)(Math.Sin(la * Math.PI / length) * 2.5 + 1.0);
                    for (int xx = (int)(x - size); xx <= (int)(x + size); xx++)
                        for (int yy = (int)(y - size); yy <= (int)(y + size); yy++)
                            for (int zz = (int)(z - size); zz <= (int)(z + size); zz++)
                            {
                                int n15 = (yy * height + zz) * width + xx;
                                if (!(0 < size * size) || xx < 1 || yy < 1 || zz < 1 || xx >= width - 1 || yy >= depth - 1 || zz >= height - 1 || blockArray[n15] != Tile.stone.id) continue;
                                blockArray[n15] = 0;
                            }
                }
            }
            client.loadingScreen("Generating level", "Watering..");
            Stopwatch sw = Stopwatch.StartNew();
            long floodFilled = 0L;
            for (int i = 0; i < width; i++)
            {
                floodFilled += floodMap(i, depth / 2 - 1, 0,          Tile.water2.id);
                floodFilled += floodMap(i, depth / 2 - 1, height - 1, Tile.water2.id);
            }
            for (int i = 0; i < height; i++)
            {
                floodFilled += floodMap(0,         depth / 2 - 1, i, Tile.water2.id);
                floodFilled += floodMap(width - 1, depth / 2 - 1, i, Tile.water2.id);
            }
            sw.Stop();
            client.loadingScreen("Generating level", "Melting..");
            int lavaCount = 0;
            for (int i = 0; i < 400; i++)
            {
                int x = random.Next(width);
                int y = random.Next(depth / 2);
                int z = random.Next(height);
                if (getTile(x, y, z) == 0)
                {
                    lavaCount++;
                    floodFilled += floodMap(x, y, z, Tile.lava2.id);
                }
            }
            Console.WriteLine("LavaCount: " + lavaCount);
            Console.WriteLine("Flood filled " + floodFilled + " tiles in " + sw.ElapsedMilliseconds + " ms");
            loadWorld(0, 0, width, height);
            for (int i = 0; i < levelListeners.Count; i++)
                levelListeners[i].createOuterChunks();
        }

        public bool loadWorld()
        {
            using (var gz = new GZipStream(new FileStream("level.dat", FileMode.OpenOrCreate), CompressionMode.Decompress))
            {
                using (var ms = new MemoryStream())
                {
                    gz.CopyTo(ms);
                    if (ms.Length == 0)
                        return false;
                    blockArray = ms.ToArray();
                }
            }
            loadWorld(0, 0, width, height);
            for (int i = 0; i < levelListeners.Count; i++)
                levelListeners[i].createOuterChunks();
            return true;
        }

        public void save()
        {
            GZipStream dos = new GZipStream(new FileStream("level.dat", FileMode.Create), CompressionLevel.Optimal);
            dos.Write(blockArray);
            dos.Flush();
            dos.Dispose();
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

        public bool checkForTile(int x, int y, int z, int id)
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

        private long floodMap(int x, int fluidLevel, int z, int id)
        {
            List<int> arrayList = new List<int>();
            int level = 1;
            int n7 = height - 1;
            int n8 = width - 1;
            fluidLevels[0] = ((fluidLevel << 8) + z << 8) + x;
            long l = 0L;
            int size = width * height;
            while (level > 0)
            {
                int n9;
                fluidLevel = fluidLevels[--level];
                if (level == 0 && arrayList.Count > 0)
                {
                    Console.WriteLine("IT HAPPENED!");
                    arrayList.RemoveAt(arrayList.Count - 1);
                    fluidLevels = arrayList.ToArray();
                    level = fluidLevels.Length;
                }
                z = fluidLevel >> 8 & n7;
                int n10 = fluidLevel >> 16;
                int n11 = n9 = fluidLevel & n8;
                while (n9 > 0 && blockArray[fluidLevel - 1] == 0)
                {
                    --n9;
                    --fluidLevel;
                }
                while (n11 < width && blockArray[fluidLevel + n11 - n9] == 0)
                    ++n11;
                int n12 = fluidLevel >> 8 & n7;
                int n13 = fluidLevel >> 16;
                if (n12 != z || n13 != n10)
                    Console.WriteLine("hoooly fuck");
                bool check1 = false;
                bool check2 = false;
                bool check3 = false;
                l += n11 - n9;
                while (n9 < n11)
                {
                    bool newCheck;
                    blockArray[fluidLevel] = (byte)id;
                    if (z > 0)
                    {
                        newCheck = blockArray[fluidLevel - width] == 0;
                        if (newCheck && !check1)
                        {
                            if (level == fluidLevels.Length)
                            {
                                arrayList.AddRange(fluidLevels);
                                Array.Clear(fluidLevels);
                                level = 0;
                            }
                            fluidLevels[level++] = fluidLevel - width;
                        }
                        check1 = newCheck;
                    }
                    if (z < height - 1)
                    {
                        newCheck = blockArray[fluidLevel + width] == 0;
                        if (newCheck && !check2)
                        {
                            if (level == fluidLevels.Length)
                            {
                                arrayList.AddRange(fluidLevels);
                                Array.Clear(fluidLevels);
                                level = 0;
                            }
                            fluidLevels[level++] = fluidLevel + width;
                        }
                        check2 = newCheck;
                    }
                    if (n10 > 0)
                    {
                        newCheck = blockArray[fluidLevel - size] == 0;
                        if (newCheck && !check3)
                        {
                            if (level == fluidLevels.Length)
                            {
                                arrayList.AddRange(fluidLevels);
                                Array.Clear(fluidLevels);
                                level = 0;
                            }
                            fluidLevels[level++] = fluidLevel - size;
                        }
                        check3 = newCheck;
                    }
                    ++fluidLevel;
                    ++n9;
                }
            }
            return l;
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
