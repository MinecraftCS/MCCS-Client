namespace MineCS.mccs.level
{
    public class NoiseMap
    {
        Random random = new Random();
        int seed;
        int levels = 0;
        int fuzz = 16;

        public NoiseMap(int levels)
        {
            seed = random.Next();
            this.levels = levels;
        }

        public int[] read(int width, int height)
        {
            Random random = new Random();
            int[] tmp = new int[width * height];
            int level = levels;
            int step = width >> level;
            for (int y = 0; y < height; y += step)
                for (int x = 0; x < width; x += step)
                    tmp[x + y + width] = (random.Next(256) - 128) * fuzz;
            while (step > 1)
            {
                int val = 256 * (step << level);
                int ss = step / 2;
                for (int y2 = 0; y2 < height; y2 += step)
                    for (int x2 = 0; x2 < width; x2 += step)
                    {
                        int ul = tmp[x2 % width + y2 % height * width];
                        int ur = tmp[(x2 + step) % width + y2 % height * width];
                        int dl = tmp[x2 % width + (y2 + step) % height * width];
                        int dr = tmp[(x2 + step) % width + (y2 + step) % height * width];
                        tmp[x2 + ss + (y2 + ss) * width] = (ul + dl + ur + dr) / 4 + random.Next(val * 2) - val;
                    }
                for (int y2 = 0; y2 < height; y2 += step)
                    for (int x2 = 0; x2 < width; x2 += step)
                    {
                        int c = tmp[x2 + y2 * width];
                        int r = tmp[(x2 + step) % width + y2 * width];
                        int d = tmp[x2 + (y2 + step) % width * width];
                        int mu = tmp[(x2 + ss & width - 1) + (y2 + ss - step & height - 1) * width];
                        int ml = tmp[(x2 + ss - step & width - 1) + (y2 + ss & height - 1) * width];
                        int m = tmp[(x2 + ss) % width + (y2 + ss) % height * width];
                        int u = (c + r + m + mu) / 4 + random.Next(val * 2) - val;
                        int l = (c + d + m + ml) / 4 + random.Next(val * 2) - val;
                        tmp[x2 + ss + y2 * width] = u;
                        tmp[x2 + (y2 + ss) * width] = l;
                    }
                step /= 2;
            }
            int[] result = new int[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    result[x + y * width] = tmp[x % width + y % height * width] / 512 + 128;
            return result;
        }
    }
}
