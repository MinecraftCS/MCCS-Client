using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using OpenTK.Graphics.OpenGL;
using MineCS.mccs.renderer;

namespace MineCS.mccs.gui
{
    public class Font
    {
        private int[] charWidths = new int[256];
        private int fontTexture = 0;

        public Font(string name, Textures textures)
        {
            Image<A8> image = Image.Load<A8>("Resources\\" + name);
            byte[] pixels = new byte[image.Width * image.Height];
            image.CopyPixelDataTo(pixels);
            for (int i = 0; i < 128; i++)
            {
                int xt = i % 16;
                int yt = i / 16;
                bool emptyColumn = false;
                int x;
                for (x = 0; x < 8 && !emptyColumn; x++)
                {
                    int xPixel = xt * 8 + x;
                    emptyColumn = true;
                    for (int y = 0; y < 8 && emptyColumn; y++)
                    {
                        int yPixel = (yt * 8 + y) * image.Width;
                        int pixel = pixels[xPixel + yPixel] & 0xFF;
                        if (pixel > 128)
                            emptyColumn = false;
                    }
                }
                if (i == 32)
                    x = 4;
                charWidths[i] = x;
            }
            fontTexture = textures.loadTexture(name, 9728);
        }

        public void drawShadow(string str, int x, int y, int color)
        {
            draw(str, x + 1, y + 1, color, true);
            draw(str, x, y, color);
        }

        public void draw(string str, int x, int y, int color)
        {
            draw(str, x, y, color, false);
        }

        public void draw(string str, int x, int y, int color, bool darken)
        {
            char[] chars = str.ToCharArray();
            if (darken)
                color = (color & 0xFCFCFC) >> 2;
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, fontTexture);
            Tesselator t = Tesselator.instance;
            t.init();
            t.color(color);
            int xo = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '&')
                {
                    int cc = "0123456789abcdef".IndexOf(chars[i + 1]);
                    int br = (cc & 8) * 8;
                    int b = (cc & 1) * 191 + br;
                    int g = ((cc & 2) >> 1) * 191 + br;
                    int r = ((cc & 4) >> 2) * 191 + br;
                    color = r << 16 | g << 8 | b;
                    i += 2;
                    if (darken)
                        color = (color & 0xFCFCFC) >> 2;
                    t.color(color);
                }
                else if (chars[i] == '\n')
                {
                    y += 10;
                    xo = 0;
                    i++;
                }
                int ix = chars[i] % 16 * 8;
                int iy = chars[i] / 16 * 8;
                t.vertexUV(x + xo,     y + 8, 0.0f, ix       / 128.0f, (iy + 8) / 128.0f);
                t.vertexUV(x + xo + 8, y + 8, 0.0f, (ix + 8) / 128.0f, (iy + 8) / 128.0f);
                t.vertexUV(x + xo + 8, y,     0.0f, (ix + 8) / 128.0f, iy       / 128.0f);
                t.vertexUV(x + xo,     y,     0.0f, ix       / 128.0f, iy       / 128.0f);
                xo += charWidths[chars[i]];
            }
            t.flush();
            GL.Disable(EnableCap.Texture2D);
        }

        public int width(string str)
        {
            int len = 0;
            foreach (string line in str.Split('\n'))
            {
                int tempLen = 0;
                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i] == '&')
                        i++;
                    else
                        tempLen += charWidths[line[i]];
                }
                len = Math.Max(len, tempLen);
            }
            return len;
        }
    }
}
