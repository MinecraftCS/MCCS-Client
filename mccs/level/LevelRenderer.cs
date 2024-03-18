using MineCS.mccs.level.tile;
using MineCS.mccs.phys;
using MineCS.mccs.renderer;
using OpenTK.Graphics.OpenGL;

namespace MineCS.mccs.level
{
    public class LevelRenderer : LevelListener
    {
        private static int MAX_REBUILDS_PER_FRAME = 8;
        private static int CHUNK_SIZE = 16;
        private Level level;
        private Chunk[] chunks;
        private int xChunks;
        private int yChunks;
        private int zChunks;
        private Textures textures;

        public LevelRenderer(Level level, Textures textures)
        {
            this.level = level;
            this.textures = textures;
            level.addListener(this);
            xChunks = level.width / 16;
            yChunks = level.depth / 16;
            zChunks = level.height / 16;
            chunks = new Chunk[xChunks * yChunks * zChunks];
            for (int x = 0; x < xChunks; x++)
                for (int y = 0; y < yChunks; y++)
                    for (int z = 0; z < zChunks; z++)
                    {
                        int x0 = x * 16;
                        int y0 = y * 16;
                        int z0 = z * 16;
                        int x1 = (x + 1) * 16;
                        int y1 = (y + 1) * 16;
                        int z1 = (z + 1) * 16;
                        if (x1 > level.width)
                            x1 = level.width;
                        if (y1 > level.depth)
                            y1 = level.depth;
                        if (z1 > level.height)
                            z1 = level.height;
                        chunks[(x + y * xChunks) * zChunks + z] = new Chunk(level, x0, y0, z0, x1, y1, z1);
                    }
        }

        public List<Chunk> getAllDirtyChunks()
        {
            List<Chunk> dirty = null;
            for (int i = 0; i < chunks.Length; i++)
            {
                Chunk chunk = chunks[i];
                if (chunk.isDirty())
                {
                    if (dirty == null)
                        dirty = new List<Chunk>();
                    dirty.Add(chunk);
                }
            }
            return dirty;
        }

        public void render(Entity player, int layer)
        {
            GL.Enable(EnableCap.Texture2D);
            int id = textures.loadTexture("/terrain.png", 9728);
            GL.BindTexture(TextureTarget.Texture2D, id);
            Frustum frustum = Frustum.getFrustum();
            for (int i = 0; i < chunks.Length; i++)
            {
                if (frustum.cubeInFrustum((chunks[i]).aabb))
                    chunks[i].render(layer);
            }
            GL.Disable(EnableCap.Texture2D);
        }

        public void updateDirtyChunks(Player player)
        {
            List<Chunk> dirty = getAllDirtyChunks();
            if (dirty == null) return;
            dirty.Sort(new DirtyChunkSorter(player, Frustum.getFrustum()));
            for (int i = 0; i < 8 && i < dirty.Count; i++)
                dirty[i].rebuild();
        }
        
        public void pick(Player player, Frustum frustum)
        {
            Tesselator t = Tesselator.instance;
            float reach = 3.0f;
            AABB box = player.bb.grow(reach, reach, reach);
            int x0 = (int)box.x0;
            int x1 = (int)(box.x1 + 1.0f);
            int y0 = (int)box.y0;
            int y1 = (int)(box.y1 + 1.0f);
            int z0 = (int)box.z0;
            int z1 = (int)(box.z1 + 1.0f);
            GL.InitNames();
            GL.PushName(0);
            GL.PushName(0);
            for (int x = x0; x < x1; x++)
            {
                GL.LoadName(x);
                GL.PushName(0);
                for (int y = y0; y < y1; y++)
                {
                    GL.LoadName(y);
                    GL.PushName(0);
                    for (int z = z0; z < z1; z++)
                    {
                        Tile tile = Tile.tiles[level.getTile(x, y, z)];
                        if (tile != null && frustum.cubeInFrustum(tile.getTileAABB(x, y, z)))
                        {
                            GL.LoadName(z);
                            GL.PushName(0);
                            for (int i = 0; i < 6; i++)
                            {
                                GL.LoadName(i);
                                t.init();
                                tile.renderFaceNoTexture(t, x, y, z, i);
                                t.flush();
                            }
                            GL.PopName();
                        }
                    }
                    GL.PopName();
                }
                GL.PopName();
            }
            GL.PopName();
            GL.PopName();
        }

        public void renderHit(HitResult h, int mode, int tileType)
        {
            Tesselator t = Tesselator.instance;
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
            GL.Color4(1.0f, 1.0f, 1.0f, ((float)Math.Sin(DateTime.UtcNow.Millisecond / 100.0) * 0.2f + 0.4f) * 0.5f);
            t.init();
            Tile.rock.renderFaceNoTexture(t, h.x, h.y, h.z, h.f);
            t.flush();
            if (mode == 0)
            {
                t.init();
                for (int i = 0; i < 6; i++)
                    Tile.rock.renderFaceNoTexture(t, h.x, h.y, h.z, i);
                t.flush();
            }
            else
            {
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                float br = (float)Math.Sin(DateTime.UtcNow.Millisecond / 100.0) * 0.2f + 0.8f;
                GL.Color4(br, br, br, Math.Sin(DateTime.UtcNow.Millisecond / 200.0) * 0.2f + 0.5f);
                GL.Enable(EnableCap.Texture2D);
                int id = textures.loadTexture("/terrain.png", 9728);
                GL.BindTexture(TextureTarget.Texture2D, id);
                int x = h.x;
                int y = h.y;
                int z = h.z;
                if (h.f == 0)
                    y--;
                if (h.f == 1)
                    y++;
                if (h.f == 2)
                    z--;
                if (h.f == 3)
                    z++;
                if (h.f == 4)
                    x--;
                if (h.f == 5)
                    x++;
                t.init();
                t.NoColor();
                Tile.tiles[tileType].render(t, level, 0, x, y, z);
                Tile.tiles[tileType].render(t, level, 1, x, y, z);
                t.flush();
                GL.Disable(EnableCap.Texture2D);
            }
            GL.Disable(EnableCap.Blend);
        }

        public void setDirty(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            x0 /= 16;
            x1 /= 16;
            y0 /= 16;
            y1 /= 16;
            z0 /= 16;
            z1 /= 16;
            if (x0 < 0)
                x0 = 0;
            if (y0 < 0)
                y0 = 0;
            if (z0 < 0)
                z0 = 0;
            if (x1 >= xChunks)
                x1 = xChunks - 1;
            if (y1 >= yChunks)
                y1 = yChunks - 1;
            if (z1 >= zChunks)
                z1 = zChunks - 1;
            for (int x = x0; x <= x1; x++)
                for (int y = y0; y <= y1; y++)
                    for (int z = z0; z <= z1; z++)
                        chunks[(x + y * xChunks) * zChunks + z].setDirty();
        }

        public void tileChanged(int x, int y, int z)
        {
            setDirty(x - 1, y - 1, z - 1, x + 1, y + 1, z + 1);
        }

        public void lightColumnChanged(int x, int z, int y0, int y1)
        {
            setDirty(x - 1, y0 - 1, z - 1, x + 1, y1 + 1, z + 1);
        }

        public void allChanged()
        {
            setDirty(0, 0, 0, level.width, level.depth, level.height);
        }
    }
}
