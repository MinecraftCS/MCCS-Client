namespace MineCS.mccs.level.generator.noise
{
    public class NoiseGenerator : Noise
    {
        private Noise noise1;
        private Noise noise2;

        public NoiseGenerator(Noise noise1, Noise noise2)
        {
            this.noise1 = noise1;
            this.noise2 = noise2;
        }

        public override double getNoise(double deci1, double deci2)
        {
            return noise1.getNoise(deci1 + noise2.getNoise(deci1, deci2), deci2);
        }
    }
}
