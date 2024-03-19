using OpenTK.Graphics.OpenGL;

namespace MineCS.mccs.renderer
{
    public class Tesselator
    {
        public const int MAX_VERTICES = 0x400000;
        public const int MAX_FLOATS = 0x80000;
        
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
        private bool lockColor = false;
        public static Tesselator instance = new Tesselator();

        public void flush()
        {
            if (vertices > 0)
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
            }
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
            lockColor = false;
        }

        public void tex(float u, float v)
        {
            if (!hasTexture)
                len += 2;
            hasTexture = true;
            this.u = u;
            this.v = v;
        }

        public void color(int r, int g, int b)
        {
            color((byte)r, (byte)g, (byte)b);
        }

        public void color(byte r, byte g, byte b)
        {
            if (lockColor) return;
            if (!hasColor)
                len += 3;
            hasColor = true;
            this.r = (r & 0xFF) / 255.0f;
            this.g = (g & 0xFF) / 255.0f;
            this.b = (b & 0xFF) / 255.0f;
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
            if (vertices % 4 == 0 && p >= MAX_FLOATS - len * 4)
                flush();
        }

        public void color(int c)
        {
            int r = c >> 16 & 0xFF;
            int g = c >> 8  & 0xFF;
            int b = c       & 0xFF;
            color(r, g, b);
        }

        public void LockColor()
        {
            lockColor = true;
        }
    }
}
