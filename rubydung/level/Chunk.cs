using MineCS.rubydung.physics;
using OpenTK.Graphics.OpenGL;

namespace MineCS.rubydung.level
{
    public class Chunk
    {
        public AABB aabb;
        public Level level;
        public int x0;
        public int y0;
        public int z0;
        public int x1;
        public int y1;
        public int z1;
        private bool dirty = true;
        private int lists = -1;
        private static Tesselator t = new Tesselator();
        public static int rebuiltThisFrame = 0;
        public static int updates = 0;

        public Chunk(Level level, int x0, int y0, int z0, int x1, int y1, int z1)
        {
            this.level = level;
            this.x0 = x0;
            this.y0 = y0;
            this.z0 = z0;
            this.x1 = x1;
            this.y1 = y1;
            this.z1 = z1;
            aabb = new AABB(x0, y0, z0, x1, y1, z1);
            lists = GL.GenLists(2);
        }

        private void rebuild(int layer)
        {
            if (rebuiltThisFrame == 2)
                return;
            dirty = false;
            updates++;
            rebuiltThisFrame++;
            int texture = Textures.loadTexture("/terrain.png", 9728);
            GL.NewList(lists + layer, ListMode.Compile);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            t.init();
            int tiles = 0;
            for (int x = x0; x < x1; x++)
                for (int y = y0; y < y1; y++)
                    for (int z = z0; z < z1; z++)
                        if (level.isTile(x, y, z))
                        {
                            bool tex = y == level.depth * 2 / 3;
                            tiles++;
                            if (tex)
                                Tile.rock.render(t, level, layer, x, y, z);
                            else
                                Tile.grass.render(t, level, layer, x, y, z);
                        }
            t.flush();
            GL.Disable(EnableCap.Texture2D);
            GL.EndList();
        }

        public void render(int layer)
        {
            if (dirty)
            {
                rebuild(0);
                rebuild(1);
            }
            GL.CallList(lists + layer);
        }

        public void setDirty()
        {
            dirty = true;
        }
    }
}
