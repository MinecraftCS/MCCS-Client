using MineCS.mccs.level.tile;
using MineCS.mccs.phys;
using MineCS.mccs.renderer;
using OpenTK.Graphics.OpenGL;

namespace MineCS.mccs.level
{
    public class LevelRenderer
    {
        public Level level;
        public Chunk[] publicChunks;
        private Chunk[] chunks;
        private int xChunks;
        private int yChunks;
        private int zChunks;
        private Textures textures;
        public int listIndex;
        public int renderDistance = 0;
        private float x = 0.0f;
        private float y = 0.0f;
        private float z = 0.0f;

        public LevelRenderer(Level level, Textures textures)
        {
            this.level = level;
            this.textures = textures;
            level.levelListeners.Add(this);
            xChunks = (level.width + 16 - 1) / 16;
            yChunks = (level.depth + 16 - 1) / 16;
            zChunks = (level.height + 16 - 1) / 16;
            publicChunks = new Chunk[xChunks * yChunks * zChunks];
            chunks = new Chunk[xChunks * yChunks * zChunks];
            for (int x = 0; x < xChunks; x++)
                for (int y = 0; y < yChunks; y++)
                    for (int z = 0; z < zChunks; z++)
                    {
                        int x0 = x << 4;
                        int y0 = y << 4;
                        int z0 = z << 4;
                        int x1 = x + 1 << 4;
                        int y1 = y + 1 << 4;
                        int z1 = z + 1 << 4;
                        if (x1 > level.width)
                            x1 = level.width;
                        if (y1 > level.depth)
                            y1 = level.depth;
                        if (z1 > level.height)
                            z1 = level.height;
                        chunks[(x + y * xChunks) * zChunks + z] = publicChunks[(x + y * xChunks) * zChunks + z] = new Chunk(level, x0, y0, z0, x1, y1, z1);
                    }
            listIndex = GL.GenLists(2);
            GL.NewList(listIndex, ListMode.Compile);
            outerChunkRenderer();
            GL.EndList();
            GL.NewList(listIndex + 1, ListMode.Compile);
            outerChunkWaterRenderer();
            GL.EndList();
        }

        public void render(Player player, int layer)
        {
            GL.Enable(EnableCap.Texture2D);
            int id = textures.loadTexture("/terrain.png", 9728);
            GL.BindTexture(TextureTarget.Texture2D, id);
            float dx = player.x - x;
            float dy = player.y - y;
            float dz = player.z - z;
            if (dx * dx + dy * dy + dz * dz > 64.0f)
            {
                x = player.x;
                y = player.y;
                z = player.z;
            }
            for (int i = 0; i < chunks.Length; i++)
            {
                if (!chunks[i].loaded) continue;
                float dist = 256 / (1 << renderDistance);
                if (renderDistance != 0 && !(chunks[i].distanceToSqr(player) < dist * dist)) continue;
                chunks[i].render(layer);
            }
            GL.Disable(EnableCap.Texture2D);
        }

        private void outerChunkRenderer()
        {
            GL.Enable(EnableCap.Texture2D);
            int id = textures.loadTexture("/rock.png", 9728);
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
            Tesselator t = Tesselator.instance;
            float size = 32.0f - 2.0f;
            t.init();
            for (int x = -640; x < level.width + 640; x += 128)
            {
                for (int y = -640; y < level.height + 640; y += 128)
                {
                    float newSize = size;
                    if (x >= 0 && y >= 0 && x < level.width && y < level.height)
                        newSize = 0.0f;
                    t.vertexUV(x, newSize, y + 128, 0.0f, 128);
                    t.vertexUV(x + 128, newSize, y + 128, 128, 128);
                    t.vertexUV(x + 128, newSize, y, 128, 0.0f);
                    t.vertexUV(x, newSize, y, 0.0f, 0.0f);
                }
            }
            t.flush();
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.Color3(0.8f, 0.8f, 0.8f);
            t.init();
            for (int x = 0; x < level.width; x += 128)
            {
                t.vertexUV(x, 0.0f, 0.0f, 0.0f, 0.0f);
                t.vertexUV(x + 128, 0.0f, 0.0f, 128, 0.0f);
                t.vertexUV(x + 128, size, 0.0f, 128, size);
                t.vertexUV(x, size, 0.0f, 0.0f, size);
                t.vertexUV(x, size, level.height, 0.0f, size);
                t.vertexUV(x + 128, size, level.height, 128, size);
                t.vertexUV(x + 128, 0.0f, level.height, 128, 0.0f);
                t.vertexUV(x, 0.0f, level.height, 0.0f, 0.0f);
            }
            GL.Color3(0.6f, 0.6f, 0.6f);
            for (int y = 0; y < level.height; y += 128)
            {
                t.vertexUV(0.0f, size, y, 0.0f, 0.0f);
                t.vertexUV(0.0f, size, y + 128, 128, 0.0f);
                t.vertexUV(0.0f, 0.0f, y + 128, 128, size);
                t.vertexUV(0.0f, 0.0f, y, 0.0f, size);
                t.vertexUV(level.width, 0.0f, y, 0.0f, size);
                t.vertexUV(level.width, 0.0f, y + 128, 128, size);
                t.vertexUV(level.width, size, y + 128, 128, 0.0f);
                t.vertexUV(level.width, size, y, 0.0f, 0.0f);
            }
            t.flush();
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
        }

        private void outerChunkWaterRenderer()
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Color3(1.0f, 1.0f, 1.0f);
            int id = textures.loadTexture("/water.png", 9728);
            GL.BindTexture(TextureTarget.Texture2D, id);
            float size = 32.0f;
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            Tesselator t = Tesselator.instance;
            t.init();
            for (int x = -640; x < level.width + 640; x += 128)
            {
                for (int y = -640; y < level.height + 640; y += 128)
                {
                    float newSize = size - 0.1f;
                    if (x >= 0 && y >= 0 && x < level.width && y < level.height) continue;
                    t.vertexUV(x, newSize, y + 128, 0.0f, 128);
                    t.vertexUV(x + 128, newSize, y + 128, 128, 128);
                    t.vertexUV(x + 128, newSize, y, 128, 0.0f);
                    t.vertexUV(x, newSize, y, 0.0f, 0.0f);
                    t.vertexUV(x, newSize, y, 0.0f, 0.0f);
                    t.vertexUV(x + 128, newSize, y, 128, 0.0f);
                    t.vertexUV(x + 128, newSize, y + 128, 128, 128);
                    t.vertexUV(x, newSize, y + 128, 0.0f, 128);
                }
            }
            t.flush();
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
        }

        public void pick(Player player, Frustum frustum)
        {
            Tesselator t = Tesselator.instance;
            float r = 3.0f;
            AABB box = player.bb.grow(r, r, r);
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
                        if (tile != null && tile.isInteractable() && frustum.cubeInFrustum(tile.getTileAABB(x, y, z)))
                        {
                            GL.LoadName(z);
                            GL.PushName(0);
                            for (int i = 0; i < 6; i++)
                            {
                                GL.LoadName(i);
                                t.init();
                                Tile.renderFaceNoTexture(player, t, x, y, z, i);
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

        public void renderHit(Player player, HitResult h, int mode, int paintTexture)
        {
            Tesselator t = Tesselator.instance;
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
            GL.Color4(1.0f, 1.0f, 1.0f, ((float)Math.Sin(DateTime.UtcNow.Millisecond / 100.0) * 0.2f + 0.4f) * 0.5f);
            if (mode == 0)
            {
                t.init();
                for (int i = 0; i < 6; i++)
                    Tile.renderFaceNoTexture(player, t, h.x, h.y, h.z, i);
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
                if (h.f == 0) y--;
                if (h.f == 1) y++;
                if (h.f == 2) z--;
                if (h.f == 3) z++;
                if (h.f == 4) x--;
                if (h.f == 5) x++;
                t.init();
                t.LockColor();
                Tile.tiles[paintTexture].render(t, level, 0, x, y, z);
                Tile.tiles[paintTexture].render(t, level, 1, x, y, z);
                t.flush();
                GL.Disable(EnableCap.Texture2D);
            }
            GL.Disable(EnableCap.Blend);
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

        public void updateDirtyChunks(Player player, Frustum frustum)
        {
            for (int i = 0; i < publicChunks.Length; i++)
                publicChunks[i].loaded = frustum.cubeInFrustum(publicChunks[i].aabb);

            List<Chunk> dirty = getAllDirtyChunks();
            if (dirty == null) return;
            dirty.Sort(new DirtyChunkSorter(player, frustum));
            for (int i = 0; i < 4 && i < dirty.Count; i++)
                dirty[i].rebuild();
        }

        public void setDirty(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            x0 /= 16;
            x1 /= 16;
            y0 /= 16;
            y1 /= 16;
            z0 /= 16;
            z1 /= 16;
            if (x0 < 0) x0 = 0;
            if (y0 < 0) y0 = 0;
            if (z0 < 0) z0 = 0;
            if (x1 >= xChunks) x1 = xChunks - 1;
            if (y1 >= yChunks) y1 = yChunks - 1;
            if (z1 >= zChunks) z1 = zChunks - 1;
            for (int x = x0; x <= x1; x++)
                for (int y = y0; y <= y1; y++)
                    for (int z = z0; z <= z1; z++)
                        publicChunks[(x + y * xChunks) * zChunks + z].setDirty();
        }

        public void createOuterChunks()
        {
            for (int i = 0; i < publicChunks.Length; i++)
                publicChunks[i].setOuter();
        }
    }
}
