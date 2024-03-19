namespace MineCS.mccs.level
{
    public class NoiseParams
    {
        public int width;
        public int height;
        public int depth;
        public Random random = new Random();

        public NoiseParams(int width, int height, int depth)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;
        }
    }
}
