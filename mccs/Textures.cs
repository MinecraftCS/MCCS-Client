using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace MineCS.mc
{
    public class Textures
    {
        private static Dictionary<string, int> idMap = new Dictionary<string, int>();
        private static int lastId = -9999999;

        public static int loadTexture(string resourceName, int mode)
        {
            if (idMap.ContainsKey(resourceName))
                return idMap[resourceName];
            int[] ib = new int[1];
            GL.GenTextures(ib.Length, ib);
            int id = ib[0];
            bind(id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            Image<Rgba32> image = Image.Load<Rgba32>("Resources\\" + resourceName);
            byte[] pixels = new byte[4 * image.Width * image.Height];
            image.CopyPixelDataTo(pixels);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width,
                            image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            Glu.Build2DMipmap(TextureTarget.Texture2D, 6408, image.Width, image.Height,
                                PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            return id;
        }
  
        public static void bind(int id)
        {
            if (id != lastId)
            {
                GL.BindTexture(TextureTarget.Texture2D, id);
                lastId = id;
            }
        }
    }
}
