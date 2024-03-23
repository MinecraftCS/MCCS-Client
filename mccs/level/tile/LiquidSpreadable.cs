namespace MineCS.mccs.level.tile
{
    public class LiquidSpreadable : Liquid
    {
        public LiquidSpreadable(int id, int type) : base(id, type)
        {
            liquidId = id - 1;
            liquidIdIndexed = id;
            setDoTick(false);
        }

        public override void tick(Level level, int x, int y, int z, Random random) { }

        public override void checkFaces(Level level, int x, int y, int z, int id)
        {
            bool canSpread = false;
            if (level.getTile(x - 1, y, z) == 0)
                canSpread = true;
            else if (level.getTile(x + 1, y, z) == 0)
                canSpread = true;
            else if (level.getTile(x, y, z - 1) == 0)
                canSpread = true;
            else if (level.getTile(x, y, z + 1) == 0)
                canSpread = true;
            else if (level.getTile(x, y - 1, z) == 0)
                canSpread = true;
            if (canSpread)
                level.tryReplaceTile(x, y, z, liquidId);
            base.checkFaces(level, x, y, z, id);
        }
    }
}
