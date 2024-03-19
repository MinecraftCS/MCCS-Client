namespace MineCS.mccs
{
    public class HitResult
    {
        public int x;
        public int y;
        public int z;
        public int f;

        public HitResult(int type, int x, int y, int z, int f)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.f = f;
        }

        public float distanceFromBlock(Player player, int mode)
        {
            int x = this.x;
            int y = this.y;
            int z = this.z;
            if (mode == 1)
            {
                if (f == 0) y--;
                if (f == 1) y++;
                if (f == 2) z--;
                if (f == 3) z++;
                if (f == 4) x--;
                if (f == 5) x++;
            }
            float vx = x - player.x;
            float vy = y - player.y;
            float vz = z - player.z;
            return vx * vx + vy * vy + vz * vz;
        }
    }
}
