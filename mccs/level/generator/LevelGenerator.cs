using MineCS.mccs.level.generator.noise;
using MineCS.mccs.level.tile;
using System.Diagnostics;

namespace MineCS.mccs.level.generator
{
    public class LevelGenerator
    {
        private Client client;
        private int width;
        private int height;
        private int depth;
        private Random random = new Random();
        private byte[] blockArray;
        private int[] fluidLevels = new int[0x100000];

        public LevelGenerator(Client client)
        {
            this.client = client;
        }

        public bool loadWorld(Level level, string username, int width, int height, int depth)
        {
            client.loadingScreenHeader("Generating level");
            this.width = width;
            this.height = height;
            this.depth = depth;
            blockArray = new byte[this.width * this.height * this.depth];
            client.loadingScreen("Raising..");
            NoiseGenerator gen1 = new NoiseGenerator(new NoiseSlicer(random, 8), new NoiseSlicer(random, 8));
            NoiseGenerator gen2 = new NoiseGenerator(new NoiseSlicer(random, 8), new NoiseSlicer(random, 8));
            Noise Nclient = new NoiseSlicer(random, 8);
            int[] blocks = new int[this.width * this.height];
            for (int x = 0; x < this.width; x++)
            {
                loadProgress(x * 100 / (this.width - 1));
                for (int z = 0; z < this.height; z++)
                {
                    double d2;
                    double d3 = gen1.getNoise(x, z) / 8.0 - 8.0;
                    double d4 = gen2.getNoise(x, z) / 8.0 + 8.0;
                    double d5 = Nclient.getNoise(x, z) / 8.0;
                    if (d5 > 2.0)
                        d4 = d3;
                    double d6 = Math.Max(d3, d4);
                    d6 = (d6 * d6 * d6 / 100.0 + d6 * 3.0) / 8.0;
                    blocks[x + z * this.width] = (int)d6;
                }
            }
            client.loadingScreen("Eroding..");
            gen1 = new NoiseGenerator(new NoiseSlicer(random, 8), new NoiseSlicer(random, 8));
            gen2 = new NoiseGenerator(new NoiseSlicer(random, 8), new NoiseSlicer(random, 8));
            for (int x = 0; x < this.width; x++)
            {
                loadProgress(x * 100 / (this.width - 1));
                for (int z = 0; z < this.height; z++)
                {
                    double d7 = gen1.getNoise(x << 1, z << 1) / 8.0;
                    int n7 = gen2.getNoise(x << 1, z << 1) > 0.0 ? 1 : 0;
                    if (d7 > 2.0)
                    {
                        int var16_31 = blocks[x + z * this.width];
                        blocks[x + z * this.width] = ((var16_31 - n7) / 2 << 1) + n7;
                    }
                }
            }
            client.loadingScreen("Soiling..");
            for (int x = 0; x < this.width; x++)
            {
                loadProgress(x * 100 / (this.width - 1));
                for (int y = 0; y < this.depth; y++)
                    for (int z = 0; z < this.height; z++)
                    {
                        int n5 = (y * this.height + z) * this.width + x;
                        int var16_32 = blocks[x + z * this.width] + this.depth / 2;
                        int n10 = 0;
                        if (y == var16_32 && y >= this.depth / 2 - 1)
                            n10 = Tile.grass.id;
                        else if (y <= var16_32)
                            n10 = Tile.dirt.id;
                        if (y <= var16_32 - 2)
                            n10 = Tile.stone.id;
                        blockArray[n5] = (byte)n10;
                    }
            }
            client.loadingScreen("Carving..");
            int count = this.width * this.height * this.depth / Math.Max(this.width, this.height) / this.depth;
            for (int i = 0; i < count; i++)
            {
                loadProgress(i * 100 / (this.width - 1));
                float x = random.NextSingle() * this.width;
                float y = random.NextSingle() * this.depth;
                float z = random.NextSingle() * this.height;
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
                                int n15;
                                if (!(0 < size * size) || xx < 1 || yy < 1 || zz < 1 || xx >= this.width - 1 || yy >= this.depth - 1 || zz >= this.height - 1 || blockArray[n15 = (yy * this.height + zz) * this.width + xx] != Tile.stone.id) continue;
                                blockArray[n15] = 0;
                            }
                }
            }
            client.loadingScreen("Watering..");
            Stopwatch sw = Stopwatch.StartNew();
            long floodFilled = 0L;
            loadProgress(0);
            for (int i = 0; i < this.width; i++)
            {
                floodFilled += floodMap(i, this.depth / 2 - 1, 0, Tile.waterSpreadable.id);
                floodFilled += floodMap(i, this.depth / 2 - 1, this.height - 1, Tile.waterSpreadable.id);
            }
            for (int i = 0; i < this.height; i++)
            {
                floodFilled += floodMap(0, this.depth / 2 - 1, i, Tile.waterSpreadable.id);
                floodFilled += floodMap(this.width - 1, this.depth / 2 - 1, i, Tile.waterSpreadable.id);
            }
            int n4 = this.width * this.height / 200;
            for (int i = 0; i < n4; i++)
            {
                int n15;
                if (i % 100 == 0)
                    loadProgress(i * 100 / (n4 - 1));
                int n16 = random.Next(this.width);
                int n17 = this.depth / 2 - 1 - random.Next(3);
                if (blockArray[(n17 * this.height + (n15 = random.Next(this.height))) * this.width + n16] != 0) continue;
                floodFilled += floodMap(n16, n17, n15, Tile.waterSpreadable.id);
            }
            loadProgress(100);
            sw.Stop();
            Console.WriteLine("Flood filled " + floodFilled + " tiles in " + sw.ElapsedMilliseconds + " ms");
            client.loadingScreen("Melting..");
            floodMap();
            level.load(this.width, this.depth, this.height, blockArray);
            level.creationTime = (long)DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
            level.username = username;
            level.name = "A Nice World";
            return true;
        }

        private void loadProgress(int progress) => client.loadingScreen(progress);

        private void floodMap()
        {
            int lavaCount = 0;
            int count = width * height * depth / 10000;
            for (int i = 0; i < count; i++)
            {
                int n6;
                if (i % 100 == 0)
                    loadProgress(i * 100 / (Math.Max(2, count) - 1));
                int n4 = random.Next(width);
                int n5 = random.Next(depth / 2 - 4);
                if (blockArray[(n5 * height + (n6 = random.Next(height))) * width + n4] != 0) continue;
                lavaCount++;
                floodMap(n4, n5, n6, Tile.lavaSpreadable.id);
            }
            loadProgress(100);
            Console.WriteLine("LavaCount: " + lavaCount);
        }

        private long floodMap(int x, int fluidLevel, int z, int id)
        {
            List<int> arrayList = new List<int>();
            int level = 1;
            int n7 = 1;
            int n8 = 1;
            while (1 << n7 < width) n7++;
            while (1 << n8 < height) n8++;
            int n9 = height - 1;
            int n10 = width - 1;
            fluidLevels[0] = ((fluidLevel << n8) + z << n7) + x;
            long l = 0L;
            int size = width * height;
            while (level > 0)
            {
                int n11;
                fluidLevel = fluidLevels[--level];
                if (level == 0 && arrayList.Count > 0)
                {
                    Console.WriteLine("IT HAPPENED!");
                    arrayList.RemoveAt(arrayList.Count - 1);
                    fluidLevels = arrayList.ToArray();
                    level = fluidLevels.Length;
                }
                z = fluidLevel >> n7 & n9;
                int n12 = fluidLevel >> n7 + n8;
                int n13 = n11 = fluidLevel & n10;
                while (n11 > 0 && blockArray[fluidLevel - 1] == 0)
                {
                    --n11;
                    --fluidLevel;
                }
                while (n13 < width && blockArray[fluidLevel + n13 - n11] == 0)
                    ++n13;
                int n14 = fluidLevel >> n7 & n9;
                int n15 = fluidLevel >> n7 + n8;
                if (n14 != z || n15 != n12)
                    Console.WriteLine("hoooly fuck");
                bool check1 = false;
                bool check2 = false;
                bool check3 = false;
                l += n13 - n11;
                while (n11 < n13)
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
                    ++n11;
                }
            }
            return l;
        }
    }
}
