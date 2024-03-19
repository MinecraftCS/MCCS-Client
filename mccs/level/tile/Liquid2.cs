namespace MineCS.mccs.level.tile
{
    // I don't know what to call this, LiquidDynamic, MixableLiquid, LiquidFuckarydoo?
    // Please help 😭
    public class Liquid2 : Liquid
    {
        public Liquid2(int id, int type) : base(id, type)
        {
            liquidId = id - 1;
            liquidIdIndexed = id;
            setDoTick(false);
        }

        public override void tick(Level level, int x, int y, int z, Random random) { }

        public override void checkFaces(Level level, int x, int y, int z, int id)
        {
            bool touchingWater = false;
            if (level.getTile(x - 1, y, z) == 0)
                touchingWater = true;
            else if (level.getTile(x + 1, y, z) == 0)
                touchingWater = true;
            else if (level.getTile(x, y, z - 1) == 0)
                touchingWater = true;
            else if (level.getTile(x, y, z + 1) == 0)
                touchingWater = true;
            else if (level.getTile(x, y - 1, z) == 0)
                touchingWater = true;
            if (touchingWater)
                level.checkForTile(x, y, z, liquidId);
            if (type == 1 && id == lava.id)
                level.checkForTile(x, y, z, stone.id);
            if (type == 2 && id == water.id)
                level.checkForTile(x, y, z, stone.id);
        }
    }
}
