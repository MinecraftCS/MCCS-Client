using MineCS.mccs.particle;
using MineCS.mccs.phys;
using MineCS.mccs.renderer;

namespace MineCS.mccs.level.tile
{
    public class Tile
    {
        public static Tile[] tiles = new Tile[256];
        public static bool[] doTick = new bool[256];
        public static Tile stone = new Tile(1, 1);
        public static Tile grass = new GrassTile(2);
        public static Tile dirt = new DirtTile(3, 2);
        public static Tile cobblestone = new Tile(4, 16);
        public static Tile log = new Tile(5, 4);
        public static Tile sapling = new Bush(6);
        public static Tile bedrock = new Tile(7, 17);
        public static Tile water = new Liquid(8, 1);
        public static Tile water2 = new Liquid2(9, 1);
        public static Tile lava = new Liquid(10, 2);
        public static Tile lava2 = new Liquid2(11, 2);
        public int tex;
        public int id;

        protected Tile(int id)
        {
            tiles[id] = this;
            this.id = id;
        }

        protected void setDoTick(bool doTick)
        {
            Tile.doTick[id] = doTick;
        }

        protected Tile(int id, int tex) : this(id)
        {
            this.tex = tex;
        }

        public virtual void render(Tesselator t, Level level, int layer, int x, int y, int z)
        {
            t.color(254, 254, 254);
            if (shouldRenderFace(level, x, y - 1, z, layer))
                renderFace(t, x, y, z, 0);
            if (shouldRenderFace(level, x, y + 1, z, layer))
                renderFace(t, x, y, z, 1);

            t.color(203, 203, 203);
            if (shouldRenderFace(level, x, y, z - 1, layer))
                renderFace(t, x, y, z, 2);
            if (shouldRenderFace(level, x, y, z + 1, layer))
                renderFace(t, x, y, z, 3);

            t.color(152, 152, 152);
            if (shouldRenderFace(level, x - 1, y, z, layer))
                renderFace(t, x, y, z, 4);
            if (shouldRenderFace(level, x + 1, y, z, layer))
                renderFace(t, x, y, z, 5);
        }

        public virtual bool shouldRenderFace(Level level, int x, int y, int z, int layer)
        {
            if (x < 0 || y < 0 || z < 0 || x >= level.width || y >= level.depth || z >= level.height)
                return false;
            bool check = true;
            if (layer == 2)
                return false;
            if (layer >= 0)
                check = level.isLit(x, y, z) ^ layer == 1;
            return !(tiles[level.getTile(x, y, z)] == null ? false : tiles[level.getTile(x, y, z)].isSolid()) && check;
        }

        protected virtual int getTexture(int face) => tex;

        public virtual void renderFace(Tesselator t, int x, int y, int z, int face)
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

        public void renderLiquid(Tesselator t, int x, int y, int z, int face)
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
                t.vertexUV(x1, y0, z1, u1, v1);
                t.vertexUV(x1, y0, z0, u1, v0);
                t.vertexUV(x0, y0, z0, u0, v0);
                t.vertexUV(x0, y0, z1, u0, v1);
            }
            if (face == 1)
            {
                t.vertexUV(x0, y1, z1, u0, v1);
                t.vertexUV(x0, y1, z0, u0, v0);
                t.vertexUV(x1, y1, z0, u1, v0);
                t.vertexUV(x1, y1, z1, u1, v1);
            }
            if (face == 2)
            {
                t.vertexUV(x0, y0, z0, u1, v1);
                t.vertexUV(x1, y0, z0, u0, v1);
                t.vertexUV(x1, y1, z0, u0, v0);
                t.vertexUV(x0, y1, z0, u1, v0);
            }
            if (face == 3)
            {
                t.vertexUV(x1, y1, z1, u1, v0);
                t.vertexUV(x1, y0, z1, u1, v1);
                t.vertexUV(x0, y0, z1, u0, v1);
                t.vertexUV(x0, y1, z1, u0, v0);
            }
            if (face == 4)
            {
                t.vertexUV(x0, y0, z1, u1, v1);
                t.vertexUV(x0, y0, z0, u0, v1);
                t.vertexUV(x0, y1, z0, u0, v0);
                t.vertexUV(x0, y1, z1, u1, v0);
            }
            if (face == 5)
            {
                t.vertexUV(x1, y1, z1, u0, v0);
                t.vertexUV(x1, y1, z0, u1, v0);
                t.vertexUV(x1, y0, z0, u1, v1);
                t.vertexUV(x1, y0, z1, u0, v1);
            }
        }

        public static void renderFaceNoTexture(Player player, Tesselator t, int x, int y, int z, int face)
        {
            float x0 = x;
            float x1 = x + 1.0f;
            float y0 = y;
            float y1 = y + 1.0f;
            float z0 = z;
            float z1 = z + 1.0f;
            if (face == 0 && y > player.y)
            {
                t.vertex(x0, y0, z1);
                t.vertex(x0, y0, z0);
                t.vertex(x1, y0, z0);
                t.vertex(x1, y0, z1);
            }
            if (face == 1 && y < player.y)
            {
                t.vertex(x1, y1, z1);
                t.vertex(x1, y1, z0);
                t.vertex(x0, y1, z0);
                t.vertex(x0, y1, z1);
            }
            if (face == 2 && z > player.z)
            {
                t.vertex(x0, y1, z0);
                t.vertex(x1, y1, z0);
                t.vertex(x1, y0, z0);
                t.vertex(x0, y0, z0);
            }
            if (face == 3 && z < player.z)
            {
                t.vertex(x0, y1, z1);
                t.vertex(x0, y0, z1);
                t.vertex(x1, y0, z1);
                t.vertex(x1, y1, z1);
            }
            if (face == 4 && x > player.x)
            {
                t.vertex(x0, y1, z1);
                t.vertex(x0, y1, z0);
                t.vertex(x0, y0, z0);
                t.vertex(x0, y0, z1);
            }
            if (face == 5 && x < player.x)
            {
                t.vertex(x1, y0, z1);
                t.vertex(x1, y0, z0);
                t.vertex(x1, y1, z0);
                t.vertex(x1, y1, z1);
            }
        }

        public AABB getTileAABB(int x, int y, int z) => new AABB(x, y, z, x + 1, y + 1, z + 1);
        public virtual AABB getAABB(int x, int y, int z) => new AABB(x, y, z, x + 1, y + 1, z + 1);

        public virtual bool blocksLight() => true;
        public virtual bool isSolid() => true;
        public virtual bool isInteractable() => true;

        public virtual void tick(Level level, int x, int y, int z, Random random) {}

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
                        particleEngine.particles.Add(new Particle(level, xp, yp, zp, xp - x - 0.5f, yp - y - 0.5f, zp - z - 0.5f, tex));
                    }
        }

        public virtual int getType() => 0;
        public virtual void checkFaces(Level level, int x, int y, int z, int id) {}
    }
}
