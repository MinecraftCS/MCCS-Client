namespace MineCS.mccs.level.generator.noise
{
    public class NoiseSlice : Noise
    {
        private int[] heightMap = new int[512];

        public NoiseSlice() : this(new Random()) { }
        public NoiseSlice(Random random)
        {
            for (int i = 0; i < 256; i++)
                heightMap[i] = i;
            for (int i = 0; i < 256; i++)
            {
                int a = random.Next(256 - i) + i;
                int b = heightMap[i];
                heightMap[i] = heightMap[a];
                heightMap[a] = b;
                heightMap[i + 256] = heightMap[i];
            }
        }

        private static double noiseGen1(double deci)
        {
            return deci * deci * deci * (deci * (deci * 6.0 - 15.0) + 10.0);
        }

        private static double noiseGen2(double deci1, double deci2, double deci3)
        {
            return deci2 + deci1 * (deci3 - deci2);
        }

        private static double noiseGen3(int height, double deci1, double deci2, double deci3)
        {
            double d5 = (height &= 0xF) < 8 ? deci1 : deci2;
            double d7 = height < 4 ? deci2 : (height == 12 || height == 14 ? deci1 : deci3);
            return ((height & 1) == 0 ? d5 : -d5) + ((height & 2) == 0 ? d7 : -d7);
        }

        public override double getNoise(double deci1, double deci2)
        {
            double deci3 = 0.0;
            double d5 = deci2;
            double d6 = deci1;
            int height = (int)Math.Floor(d6) & 0xFF;
            int n2 = (int)Math.Floor(d5) & 0xFF;
            int n3 = (int)Math.Floor(0.0) & 0xFF;
            d6 -= Math.Floor(d6);
            d5 -= Math.Floor(d5);
            deci3 = 0.0 - Math.Floor(0.0);
            double d7 = noiseGen1(d6);
            double d8 = noiseGen1(d5);
            double d9 = noiseGen1(deci3);
            int n4 = heightMap[height] + n2;
            int n5 = heightMap[n4] + n3;
            n4 = heightMap[n4 + 1] + n3;
            height = heightMap[height + 1] + n2;
            n2 = heightMap[height] + n3;
            height = heightMap[height + 1] + n3;
            return noiseGen2(d9,
                        noiseGen2(d8,
                            noiseGen2(d7,
                                noiseGen3(heightMap[n5], d6, d5, deci3),
                                noiseGen3(heightMap[n2], d6 - 1.0, d5, deci3)),
                            noiseGen2(d7,
                                noiseGen3(heightMap[n4], d6, d5 - 1.0, deci3),
                                noiseGen3(heightMap[height], d6 - 1.0, d5 - 1.0, deci3))),
                        noiseGen2(d8,
                            noiseGen2(d7,
                                noiseGen3(heightMap[n5 + 1], d6, d5, deci3 - 1.0),
                                noiseGen3(heightMap[n2 + 1], d6 - 1.0, d5, deci3 - 1.0)),
                            noiseGen2(d7,
                                noiseGen3(heightMap[n4 + 1], d6, d5 - 1.0, deci3 - 1.0),
                                noiseGen3(heightMap[height + 1], d6 - 1.0, d5 - 1.0, deci3 - 1.0))));
        }
    }
}
