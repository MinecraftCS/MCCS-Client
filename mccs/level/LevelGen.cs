using MineCS.mccs.level.tile;

namespace MineCS.mccs.level
{
    public class LevelGen
    {
        private int width;
        private int height;
        private int depth;
        private Random random = new Random();

        public LevelGen(int width, int height, int depth)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;
        }

        public byte[] generateMap()
        {
            int w = width;
            int h = height;
            int d = depth;
            int[] heightmap1 = new NoiseMap(0).read(w, h);
            int[] heightmap2 = new NoiseMap(0).read(w, h);
            int[] cf = new NoiseMap(1).read(w, h);
            int[] rockmap = new NoiseMap(1).read(w, h);
            byte[] blocks = new byte[w * h * d];
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
            int count = w * h * d / 256 / 64;
            for (int i = 0; i < count; i++)
            {
                float x2 = random.NextSingle() * w;
                float y = random.NextSingle() * d;
                float z = random.NextSingle() * h;
                int length = (int)(random.NextSingle() + random.NextSingle() * 150.0f);
                float dir1 = (float)(random.NextSingle() * Math.PI * 2.0);
                float dira1 = 0.0f;
                float dir2 = (float)(random.NextSingle() * Math.PI * 2.0);
                float dira2 = 0.0f;
                for (int l = 0; l < length; l++)
                {
                    x2 = (float)(x2 + Math.Sin(dir1) * Math.Cos(dir2));
                    z = (float)(z + Math.Cos(dir1) * Math.Cos(dir2));
                    y = (float)(y + Math.Sin(dir2));
                    dir1 += dira1 * 0.2f;
                    dira1 *= 0.9f;
                    dira1 += random.NextSingle() - random.NextSingle();
                    dir2 += dira2 * 0.5f;
                    dir2 *= 0.5f;
                    dira2 *= 0.9f;
                    dira2 += random.NextSingle() - random.NextSingle();
                    float size = (float)(Math.Sin(l * Math.PI / length) * 2.5 + 1.0);
                    for (int xx = (int)(x2 - size); xx <= (int)(x2 + size); xx++)
                        for (int yy = (int)(y - size); yy <= (int)(y + size); yy++)
                            for (int zz = (int)(z - size); zz <= (int)(z + size); zz++)
                            {
                                int ii;
                                float xd = xx - x2;
                                float yd = yy - y;
                                float zd = zz - z;
                                float dd = xd * xd + yd * yd * 2.0f + zd * zd;
                                if (dd < size * size && xx >= 1 && yy >= 1 && zz >= 1 && xx < width - 1 && yy < depth - 1 && zz < height - 1 && blocks[ii = (yy * height + zz) * width + xx] == Tile.rock.id)
                                    blocks[ii] = 0;
                            }
                }
            }
            return blocks;
        }
    }
}
