namespace MineCS.mccs.level
{
    public class NoiseMap
    {
        private Random random;
        private int levels = 0;
        private int fuzz = 16;
        private bool alignLevels;

        public NoiseMap(Random random, int levels, bool alignLevels)
        {
            this.random = random;
            this.levels = levels;
            this.alignLevels = alignLevels;
        }


        public int[] read(int width, int height)
        {
            int[] tmp = new int[width * height];
            int level = levels;
            int step = width >> level;
            for (int y = 0; y < height; y += step)
                for (int x = 0; x < width; x += step)
                {
                    tmp[x + y * width] = (random.Next(256) - 128) * fuzz;
                    if (!alignLevels) continue;
                    if (x == 0 || y == 0)
                    {
                        tmp[x + y * width] = 0;
                        continue;
                    }
                    tmp[x + y * width] = (random.Next(192) - 64) * fuzz;
                }
            for (int w = width >> level; w > 1; w /= 2)
            {
                int val = 256 * (w << level);
                int ss = w / 2;
                for (int y2 = 0; y2 < height; y2 += w)
                    for (int x2 = 0; x2 < width; x2 += w)
                    {
                        int ul = tmp[x2 % width + y2 % height * width];
                        int ur = tmp[(x2 + w) % width + y2 % height * width];
                        int dl = tmp[x2 % width + (y2 + w) % height * width];
                        int dr = tmp[(x2 + w) % width + (y2 + w) % height * width];
                        tmp[x2 + ss + (y2 + ss) * width] = (ul + dl + ur + dr) / 4 + random.Next(val << 1) - val;
                        if (!alignLevels || x2 != 0 && y2 != 0) continue;
                        tmp[x2 + y2 * width] = 0;
                    }
                for (int y2 = 0; y2 < height; y2 += w)
                    for (int x2 = 0; x2 < width; x2 += w)
                    {
                        int c = tmp[x2 + y2 * width];
                        int r = tmp[(x2 + w) % width + y2 * width];
                        int d = tmp[x2 + (y2 + w) % width * width];
                        int mu = tmp[(x2 + ss & width - 1) + (y2 + ss - w & height - 1) * width];
                        int ml = tmp[(x2 + ss - w & width - 1) + (y2 + ss & height - 1) * width];
                        int m = tmp[(x2 + ss) % width + (y2 + ss) % height * width];
                        int u = (c + r + m + mu) / 4 + random.Next(val << 1) - val;
                        int l = (c + d + m + ml) / 4 + random.Next(val << 1) - val;
                        tmp[x2 + ss + y2 * width] = u;
                        tmp[x2 + (y2 + ss) * width] = l;
                    }
            }
            int[] result = new int[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    result[x + y * width] = tmp[x % width + y % height * width] / 512 + 128;
            return result;
        }
    }
}
