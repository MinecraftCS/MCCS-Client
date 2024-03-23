namespace MineCS.mccs.level.generator.noise
{
    public class NoiseSlicer : Noise
    {
        private NoiseSlice[] slices = new NoiseSlice[8];
        private int count = 8;

        public NoiseSlicer(Random random, int index)
        {
            for (int i = 0; i < 8; i++)
                slices[i] = new NoiseSlice(random);
        }

        public override double getNoise(double deci1, double deci2)
        {
            double d4 = 0.0;
            double d5 = 1.0;
            for (int i = 0; i < count; i++)
            {
                d4 += slices[i].getNoise(deci1 / d5, deci2 / d5) * d5;
                d5 *= 2.0;
            }
            return d4;
        }
    }
}
