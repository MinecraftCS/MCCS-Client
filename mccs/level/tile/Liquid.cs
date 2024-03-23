using MineCS.mccs.phys;
using MineCS.mccs.renderer;

namespace MineCS.mccs.level.tile
{
    public class Liquid : Tile
    {
        protected int type;
        protected int liquidIdIndexed;
        protected int liquidId;
        private int speed = 1;

        public Liquid(int id, int type) : base(id)
        {
            this.type = type;
            tex = 14;
            if (type == 1)
                speed = 8;
            else if (type == 2)
            {
                tex = 30;
                speed = 2;
            }
            liquidId = id;
            liquidIdIndexed = id + 1;
            setSize(0.0f, -0.1f, 0.0f, 1.0f, 0.9f, 1.0f);
            setDoTick(true);
        }

        public override void tick(Level level, int x, int y, int z, Random random)
        {
            trySpread(level, x, y, z, 0);
        }

        private bool trySpread(Level level, int x, int y, int z, int param)
        {
            bool didSpread = false;
            while (level.getTile(x, --y, z) == 0 && y >= 0)
            {
                bool check = level.setTile(x, y, z, liquidId);
                if (check) didSpread = true;
            }
            y++;
            if (type == 1 || !didSpread)
            {
                didSpread |= tryHoriSpread(level, x - 1, y, z, param);
                didSpread |= tryHoriSpread(level, x + 1, y, z, param);
                didSpread |= tryHoriSpread(level, x, y, z - 1, param);
                didSpread |= tryHoriSpread(level, x, y, z + 1, param);
            }
            if (!didSpread)
                level.tryReplaceTile(x, y, z, liquidIdIndexed);
            return didSpread;
        }

        private bool tryHoriSpread(Level level, int x, int y, int z, int param)
        {
            bool didSpread = false;
            int tile = level.getTile(x, y, z);
            if (tile == 0)
            {
                bool check = level.setTile(x, y, z, liquidId);
                if (check && param < speed)
                    didSpread = false | trySpread(level, x, y, z, param + 1);
            }
            return didSpread;
        }

        public override bool shouldRenderFace(Level level, int x, int y, int z, int layer, int face)
        {
            if (x < 0 || y < 0 || z < 0 || x >= level.width || z >= level.height) return false;
            if (layer != 2 && type == 1) return false;
            int tile = level.getTile(x, y, z);
            if (tile == liquidId || tile == liquidIdIndexed)
                return false;
            return base.shouldRenderFace(level, x, y, z, -1, face);
        }

        public override void renderFace(Tesselator t, int x, int y, int z, int face)
        {
            base.renderFace(t, x, y, z, face);
            renderLiquid(t, x, y, z, face);
        }

        public override bool isInteractable() => false;
        public override AABB getAABB(int x, int y, int z) => null;
        public override bool isSolid() => false;
        public override int getType() => type;

        public override void checkFaces(Level level, int x, int y, int z, int id)
        {
            if (type == 1 && id == lava.id)
                level.tryReplaceTile(x, y, z, stone.id);
            if (type == 2 && id == water.id)
                level.tryReplaceTile(x, y, z, stone.id);
        }
    }
}
