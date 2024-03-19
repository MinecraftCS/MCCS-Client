namespace MineCS.mccs.level.tile
{
    public class GrassTile : Tile
    {
        public GrassTile(int id) : base(id)
        {
            tex = 3;
            setDoTick(true);
        }

        protected override int getTexture(int face)
        {
            if (face == 1)
                return 0;
            if (face == 0)
                return 2;
            return 3;
        }

        public override void tick(Level level, int x, int y, int z, Random random)
        {
            if (random.Next(4) != 0) return;
            if (!level.isLit(x, y + 1, z))
                level.setTile(x, y, z, dirt.id);
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    int zt = z + random.Next(3) - 1;
                    int yt = y + random.Next(5) - 3;
                    int xt = x + random.Next(3) - 1;
                    if (level.getTile(xt, yt, zt) == dirt.id && level.isLit(xt, yt, zt))
                        level.setTile(xt, yt, zt, grass.id);
                }
            }
        }
    }
}
