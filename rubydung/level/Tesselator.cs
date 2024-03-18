using OpenTK.Graphics.OpenGL;

namespace MineCS.rubydung.level
{
    public class Tesselator
    {
        private static int MAX_VERTICES = 100000;
        private float[] vertexBuffer = new float[300000];
        private float[] texCoordBuffer = new float[200000];
        private float[] colorBuffer = new float[300000];
        public int vertices = 0;
        private float u;
        private float v;
        private float r;
        private float g;
        private float b;
        private bool hasColor = false;
        private bool hasTexture = false;

        public void flush()
        {
            GL.VertexPointer(3,VertexPointerType.Float, 0, vertexBuffer);
            if (hasTexture)
                GL.TexCoordPointer(2,TexCoordPointerType.Float, 0, texCoordBuffer);
            if (hasColor)
                GL.ColorPointer(3, ColorPointerType.Float, 0, colorBuffer);
            
            GL.EnableClientState(ArrayCap.VertexArray);
            if (hasTexture)
                GL.EnableClientState(ArrayCap.TextureCoordArray);
            if (hasColor)
                GL.EnableClientState(ArrayCap.ColorArray);
            GL.DrawArrays(PrimitiveType.Quads, 0, vertices);
            GL.DisableClientState(ArrayCap.VertexArray);
            if (hasTexture)
                GL.DisableClientState(ArrayCap.TextureCoordArray);
            if (hasColor)
                GL.DisableClientState(ArrayCap.ColorArray);
            clear();
        }

        private void clear()
        {
            vertices = 0;
        }

        public void init()
        {
            clear();
            hasColor = false;
            hasTexture = false;
        }

        public void tex(float u, float v)
        {
            hasTexture = true;
            this.u = u;
            this.v = v;
		}

		public void color(int col)
		{
			int r = col >> 16 & 0xFF;
			int g = col >> 8  & 0xFF;
			int b = col       & 0xFF;
			color(r, g, b);
		}

		public void color(float r, float g, float b)
        {
            hasColor = true;
            this.r = r;
            this.g = g;
            this.b = b;
		}

		public void vertex(float x, float y, float z)
        {
            vertexBuffer[vertices * 3 + 0] = x;
            vertexBuffer[vertices * 3 + 1] = y;
            vertexBuffer[vertices * 3 + 2] = z;
            if (hasTexture)
            {
                texCoordBuffer[vertices * 2 + 0] = u;
                texCoordBuffer[vertices * 2 + 1] = v;
            }
            if (hasColor)
            {
                colorBuffer[vertices * 3 + 0] = r;
                colorBuffer[vertices * 3 + 1] = g;
                colorBuffer[vertices * 3 + 2] = b;
            }
            vertices++;
            if (vertices == MAX_VERTICES)
                flush();
        }
    }
}
