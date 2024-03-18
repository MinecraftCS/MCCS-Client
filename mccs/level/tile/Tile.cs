using MineCS.mccs.particle;
using MineCS.mccs.phys;
using MineCS.mccs.renderer;

namespace MineCS.mccs.level.tile
{
    public class Tile
    {
        public static Tile[] tiles = new Tile[256];
        public static Tile empty = null;
        public static Tile rock = new Tile(1, 1);
        public static Tile grass = new GrassTile(2);
        public static Tile dirt = new DirtTile(3, 2);
        public static Tile stoneBrick = new Tile(4, 16);
        public static Tile wood = new Tile(5, 4);
        public static Tile bush = new Bush(6);
        public int tex;
        public int id;

        protected Tile(int id)
        {
            tiles[id] = this;
            this.id = id;
        }

        protected Tile(int id, int tex) : this(id)
        {
            this.tex = tex;
        }

        public virtual void render(Tesselator t, Level level, int layer, int x, int y, int z)
        {
            float c1 = 1.0f;
            float c2 = 0.8f;
            float c3 = 0.6f;
            if (shouldRenderFace(level, x, y - 1, z, layer))
            {
                t.color(c1, c1, c1);
                renderFace(t, x, y, z, 0);
            }
            if (shouldRenderFace(level, x, y + 1, z, layer))
            {
                t.color(c1, c1, c1);
                renderFace(t, x, y, z, 1);
            }
            if (shouldRenderFace(level, x, y, z - 1, layer))
            {
                t.color(c2, c2, c2);
                renderFace(t, x, y, z, 2);
            }
            if (shouldRenderFace(level, x, y, z + 1, layer))
            {
                t.color(c2, c2, c2);
                renderFace(t, x, y, z, 3);
            }
            if (shouldRenderFace(level, x - 1, y, z, layer))
            {
                t.color(c3, c3, c3);
                renderFace(t, x, y, z, 4);
            }
            if (shouldRenderFace(level, x + 1, y, z, layer))
            {
                t.color(c3, c3, c3);
                renderFace(t, x, y, z, 5);
            }
        }

        private bool shouldRenderFace(Level level, int x, int y, int z, int layer)
        {
            return !level.isSolidTile(x, y, z) && level.isLit(x, y, z) ^ layer == 1;
        }

        protected virtual int getTexture(int face)
        {
            return tex;
        }

        public void renderFace(Tesselator t, int x, int y, int z, int face)
        {
            int tex = getTexture(face);
            float u0 = tex % 16 / 16.0f;
            float u1 = u0 + 0.0624375f;
            float v0 = tex / 16 / 16.0f;
            float v1 = v0 + 0.0624375f;
            float x0 = x + 0.0f;
            float x1 = x + 1.0f;
            float y0 = y + 0.0f;
            float y1 = y + 1.0f;
            float z0 = z + 0.0f;
            float z1 = z + 1.0f;
            if (face == 0)
            {
                t.vertexUV(x0, y0, z1, u0, v1);
                t.vertexUV(x0, y0, z0, u0, v0);
                t.vertexUV(x1, y0, z0, u1, v0);
                t.vertexUV(x1, y0, z1, u1, v1);
            }
            if (face == 1)
            {
                t.vertexUV(x1, y1, z1, u1, v1);
                t.vertexUV(x1, y1, z0, u1, v0);
                t.vertexUV(x0, y1, z0, u0, v0);
                t.vertexUV(x0, y1, z1, u0, v1);
            }
            if (face == 2)
            {
                t.vertexUV(x0, y1, z0, u1, v0);
                t.vertexUV(x1, y1, z0, u0, v0);
                t.vertexUV(x1, y0, z0, u0, v1);
                t.vertexUV(x0, y0, z0, u1, v1);
            }
            if (face == 3)
            {
                t.vertexUV(x0, y1, z1, u0, v0);
                t.vertexUV(x0, y0, z1, u0, v1);
                t.vertexUV(x1, y0, z1, u1, v1);
                t.vertexUV(x1, y1, z1, u1, v0);
            }
            if (face == 4)
            {
                t.vertexUV(x0, y1, z1, u1, v0);
                t.vertexUV(x0, y1, z0, u0, v0);
                t.vertexUV(x0, y0, z0, u0, v1);
                t.vertexUV(x0, y0, z1, u1, v1);
            }
            if (face == 5)
            {
                t.vertexUV(x1, y0, z1, u0, v1);
                t.vertexUV(x1, y0, z0, u1, v1);
                t.vertexUV(x1, y1, z0, u1, v0);
                t.vertexUV(x1, y1, z1, u0, v0);
            }
        }

        public void renderFaceNoTexture(Tesselator t, int x, int y, int z, int face)
        {
            float x0 = x + 0.0f;
            float x1 = x + 1.0f;
            float y0 = y + 0.0f;
            float y1 = y + 1.0f;
            float z0 = z + 0.0f;
            float z1 = z + 1.0f;
            if (face == 0)
            {
                t.vertex(x0, y0, z1);
                t.vertex(x0, y0, z0);
                t.vertex(x1, y0, z0);
                t.vertex(x1, y0, z1);
            }
            if (face == 1)
            {
                t.vertex(x1, y1, z1);
                t.vertex(x1, y1, z0);
                t.vertex(x0, y1, z0);
                t.vertex(x0, y1, z1);
            }
            if (face == 2)
            {
                t.vertex(x0, y1, z0);
                t.vertex(x1, y1, z0);
                t.vertex(x1, y0, z0);
                t.vertex(x0, y0, z0);
            }
            if (face == 3)
            {
                t.vertex(x0, y1, z1);
                t.vertex(x0, y0, z1);
                t.vertex(x1, y0, z1);
                t.vertex(x1, y1, z1);
            }
            if (face == 4)
            {
                t.vertex(x0, y1, z1);
                t.vertex(x0, y1, z0);
                t.vertex(x0, y0, z0);
                t.vertex(x0, y0, z1);
            }
            if (face == 5)
            {
                t.vertex(x1, y0, z1);
                t.vertex(x1, y0, z0);
                t.vertex(x1, y1, z0);
                t.vertex(x1, y1, z1);
            }
        }

        public AABB getTileAABB(int x, int y, int z)
        {
            return new AABB(x, y, z, x + 1, y + 1, z + 1);
        }

        public virtual AABB getAABB(int x, int y, int z)
        {
            return new AABB(x, y, z, x + 1, y + 1, z + 1);
        }

        public virtual bool blocksLight()
        {
            return true;
        }

        public virtual bool isSolid()
        {
            return true;
        }

        public virtual void tick(Level level, int x, int y, int z, Random random) { }

        public void destroy(Level level, int x, int y, int z, ParticleEngine particleEngine)
        {
            int SD = 4;
            for (int xx = 0; xx < SD; xx++)
                for (int yy = 0; yy < SD; yy++)
                    for (int zz = 0; zz < SD; zz++)
                    {
                        float xp = x + (xx + 0.5f) / SD;
                        float yp = y + (yy + 0.5f) / SD;
                        float zp = z + (zz + 0.5f) / SD;
                        particleEngine.add(new Particle(level, xp, yp, zp, xp - x - 0.5f, yp - y - 0.5f, zp - z - 0.5f, tex));
                    }
        }
    }
}
