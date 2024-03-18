using OpenTK.Graphics.OpenGL;

namespace MineCS.mccs.level
{
    public class Tesselator
    {
        private const int MAX_VERTICES = 0x400000;
        private const int MAX_FLOATS = 0x80000;
        private float[] buffer = new float[MAX_FLOATS];
        private float[] array = new float[MAX_FLOATS];
        public int vertices = 0;
        private float u;
        private float v;
        private float r;
        private float g;
        private float b;
        private bool hasColor = false;
        private bool hasTexture = false;
        private int len = 3;
        private int p = 0;
        public static Tesselator instance = new Tesselator();

        public void flush()
        {
            Array.Resize(ref buffer, p);
            Array.Copy(array, 0, buffer, 0, p);
            if (hasTexture && hasColor)
                GL.InterleavedArrays(InterleavedArrayFormat.T2fC3fV3f, 0, buffer);
            else if (hasTexture)
                GL.InterleavedArrays(InterleavedArrayFormat.T2fV3f, 0, buffer);
            else if (hasColor)
                GL.InterleavedArrays(InterleavedArrayFormat.C3fV3f, 0, buffer);
            else
                GL.InterleavedArrays(InterleavedArrayFormat.V3f, 0, buffer);

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
            Array.Clear(buffer);
            p = 0;
        }

        public void init()
        {
            clear();
            hasColor = false;
            hasTexture = false;
        }

        public void tex(float u, float v)
        {
            if (!hasTexture)
                len += 2;
            hasTexture = true;
            this.u = u;
            this.v = v;
		}

		public void color(float r, float g, float b)
        {
            if (!hasColor)
                len += 3;
            hasColor = true;
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public void vertexUV(float x, float y, float z, float u, float v)
        {
            tex(u, v);
            vertex(x, y, z);
        }

        public void vertex(float x, float y, float z)
        {
            if (hasTexture)
            {
                array[p++] = u;
                array[p++] = v;
            }
            if (hasColor)
            {
                array[p++] = r;
                array[p++] = g;
                array[p++] = b;
            }
            array[p++] = x;
            array[p++] = y;
            array[p++] = z;
            vertices++;
            if (p >= MAX_FLOATS - len)
                flush();
        }
    }
}
