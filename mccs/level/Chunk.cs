using MineCS.mccs.level.tile;
using MineCS.mccs.phys;
using MineCS.mccs.renderer;
using OpenTK.Graphics.OpenGL;

namespace MineCS.mccs.level
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
        public float x;
        public float y;
        public float z;
        private bool dirty = true;
        private int lists = -1;
        public long dirtiedTime = 0L;
        public bool loaded;
        public bool empty;
        private static Tesselator t = Tesselator.instance;
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
            x = (x0 + x1) / 2.0f;
            y = (y0 + y1) / 2.0f;
            z = (z0 + z1) / 2.0f;
            aabb = new AABB(x0, y0, z0, x1, y1, z1);
            lists = GL.GenLists(3);
        }

        private void rebuild(int layer)
        {
            GL.NewList(lists + layer, ListMode.Compile);
            t.init();
            bool notEmpty = false;
            for (int x = x0; x < x1; x++)
                for (int y = y0; y < y1; y++)
                    for (int z = z0; z < z1; z++)
                    {
                        int tileId = level.getTile(x, y, z);
                        if (tileId > 0)
                            notEmpty |= Tile.tiles[tileId].render(t, level, layer, x, y, z);
                    }
            if (notEmpty)
                empty = false;
            t.flush();
            GL.EndList();
        }

        public void rebuild()
        {
            empty = true;
            updates++;
            rebuild(0);
            rebuild(1);
            rebuild(2);
            dirty = false;
        }

        public int getList(int layer)
        {
            return lists + layer;
        }

        public void setDirty()
        {
            if (!dirty)
                dirtiedTime = (long)(DateTime.UtcNow.TimeOfDay.TotalMilliseconds * 1000000.0);
            dirty = true;
        }

        public bool isDirty() => dirty;

        public float distanceToSqr(Player player)
        {
            float xd = player.x - x;
            float yd = player.y - y;
            float zd = player.z - z;
            return xd * xd + yd * yd + zd * zd;
        }

        public void setOuter()
        {
            dirty = true;
            for (int i = 0; i < 3; i++)
            {
                GL.NewList(lists + i, ListMode.Compile);
                GL.EndList();
            }
        }

        public void delete()
        {
            GL.DeleteLists(lists, 3);
        }
    }
}
