using OpenTK.Graphics.OpenGL;

namespace MineCS.mccs.character
{
    public class Cube
    {
        private Vertex[] vertices;
        private Polygon[] polygons;
        private int xTexOffs;
        private int yTexOffs;
        public float x;
        public float y;
        public float z;
        public float xRot;
        public float yRot;
        public float zRot;

        public Cube(int xTexOffs, int yTexOffs)
        {
            this.xTexOffs = xTexOffs;
            this.yTexOffs = yTexOffs;
        }

        public void setTexOffs(int xTexOffs, int yTexOffs)
        {
            this.xTexOffs = xTexOffs;
            this.yTexOffs = yTexOffs;
        }

        public void addBox(float x0, float y0, float z0, int w, int h, int d)
        {
            float x1 = x0 + w;
            float y1 = y0 + h;
            float z1 = z0 + d;
            Vertex u0, u1, u2, u3, l0, l1, l2, l3;
            vertices = new Vertex[8]
            {
                u0 = new Vertex(x0, y0, z0, 0.0f, 0.0f),
                u1 = new Vertex(x1, y0, z0, 0.0f, 8.0f),
                u2 = new Vertex(x1, y1, z0, 8.0f, 8.0f),
                u3 = new Vertex(x0, y1, z0, 8.0f, 0.0f),
                l0 = new Vertex(x0, y0, z1, 0.0f, 0.0f),
                l1 = new Vertex(x1, y0, z1, 0.0f, 8.0f),
                l2 = new Vertex(x1, y1, z1, 8.0f, 8.0f),
                l3 = new Vertex(x0, y1, z1, 8.0f, 0.0f),
            };
            polygons = new Polygon[6]
            {
                new Polygon(new Vertex[] { l1, u1, u2, l2 }, xTexOffs + d + w,     yTexOffs + d, xTexOffs + d + w + d,     yTexOffs + d + h),
                new Polygon(new Vertex[] { u0, l0, l3, u3 }, xTexOffs,             yTexOffs + d, xTexOffs + d,             yTexOffs + d + h),
                new Polygon(new Vertex[] { l1, l0, u0, u1 }, xTexOffs + d,         yTexOffs,     xTexOffs + d + w,         yTexOffs + d),
                new Polygon(new Vertex[] { u2, u3, l3, l2 }, xTexOffs + d + w,     yTexOffs,     xTexOffs + d + w + w,     yTexOffs + d),
                new Polygon(new Vertex[] { u1, u0, u3, u2 }, xTexOffs + d,         yTexOffs + d, xTexOffs + d + w,         yTexOffs + d + h),
                new Polygon(new Vertex[] { l0, l1, l2, l3 }, xTexOffs + d + w + d, yTexOffs + d, xTexOffs + d + w + d + w, yTexOffs + d + h),
            };
        }

        public void setPos(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void render()
        {
            float c = 57.29578f;
            GL.PushMatrix();
            GL.Translate(x, y, z);
            GL.Rotate(zRot * c, 0.0f, 0.0f, 1.0f);
            GL.Rotate(yRot * c, 0.0f, 1.0f, 0.0f);
            GL.Rotate(xRot * c, 1.0f, 0.0f, 0.0f);
            GL.Begin(PrimitiveType.Quads);
            for (int i = 0; i < polygons.Length; i++)
                polygons[i].render();
            GL.End();
            GL.PopMatrix();
        }
    }
}
