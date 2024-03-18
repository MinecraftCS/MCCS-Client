using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace MineCS.rubydung
{
    public class Textures
    {
        private static Dictionary<string, int> idMap = new Dictionary<string, int>();

        public static int loadTexture(string resourceName, int mode)
        {
            if (idMap.ContainsKey(resourceName))
                return idMap[resourceName];
            int[] ib = new int[1];
            GL.GenTextures(ib.Length, ib);
            int id = ib[0];
            idMap.Add(resourceName, id);
            Debug.WriteLine(resourceName + " -> " + id);
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, mode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, mode);
            Image<Rgba32> image = Image.Load<Rgba32>("Resources\\" + resourceName);
            byte[] pixels = new byte[4 * image.Width * image.Height];
            image.CopyPixelDataTo(pixels);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            Glu.Build2DMipmap(TextureTarget.Texture2D, 6408, image.Width, image.Height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            return id;
        }
    }
}
