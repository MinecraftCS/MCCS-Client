using MineCS.mccs.phys;
using MineCS.mccs.renderer;

namespace MineCS.mccs.level.tile
{
    public class Bush : Tile
    {
        public Bush(int id) : base(id)
        {
            tex = 15;
        }

        public override void tick(Level level, int x, int y, int z, Random random)
        {
            int below = level.getTile(x, y - 1, z);
            if (!level.isLit(x, y, z) || below != dirt.id && below != grass.id)
                level.setTile(x, y, z, 0);
        }

        public override void render(Tesselator t, Level level, int layer, int x, int y, int z)
        {
            if (level.isLit(x, y, z) ^ layer != 1)
                return;
            int tex = getTexture(15);
            float u0 = (tex % 16) / 16.0f;
            float u1 = u0 + 0.0624375f;
            float v0 = (tex / 16) / 16.0f;
            float v1 = v0 + 0.0624375f;
            int rots = 2;
            t.color(1.0f, 1.0f, 1.0f);
            for (int r = 0; r < rots; r++)
            {
                float xa = (float)(Math.Sin(r * Math.PI / rots + 0.7853981633974483) * 0.5);
                float za = (float)(Math.Cos(r * Math.PI / rots + 0.7853981633974483) * 0.5);
                float x0 = x + 0.5f - xa;
                float x1 = x + 0.5f + xa;
                float y0 = y;
                float y1 = y + 1.0f;
                float z0 = z + 0.5f - za;
                float z1 = z + 0.5f + za;
                t.vertexUV(x0, y1, z0, u1, v0);
                t.vertexUV(x1, y1, z1, u0, v0);
                t.vertexUV(x1, y0, z1, u0, v1);
                t.vertexUV(x0, y0, z0, u1, v1);
                t.vertexUV(x1, y1, z1, u0, v0);
                t.vertexUV(x0, y1, z0, u1, v0);
                t.vertexUV(x0, y0, z0, u1, v1);
                t.vertexUV(x1, y0, z1, u0, v1);
            }
        }

        public override AABB getAABB(int x, int y, int z)
        {
            return null;
        }

        public override bool blocksLight()
        {
            return false;
        }

        public override bool isSolid()
        {
            return false;
        }
    }
}
