using MineCS.mccs.renderer;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace MineCS.mccs.gui
{
    public class BaseGui
    {
        protected Client client;
        protected int screenWidth;
        protected int screenHeight;
        protected List<MenuItem> menuItems = new();

        public virtual void render(int mouseX, int mouseY)
        {
            for (int i = 0; i < menuItems.Count; i++)
            {
                MenuItem item = menuItems[i];
                if (!item.isButton) continue;
                if (!item.active)
                {
                    drawButton(item.x - 1, item.y - 1, item.x + item.width + 1, item.y + item.height + 1, 0xFF8080A0);
                    drawButton(item.x, item.y, item.x + item.width, item.y + item.height, 0xFF909090);
                    renderTextFromRight(item.label, item.x + item.width / 2, item.y + (item.height - 8) / 2, 0xA0A0A0);
                    continue;
                }
                drawButton(item.x - 1, item.y - 1, item.x + item.width + 1, item.y + item.height + 1, 0xFF000000);
                if (mouseX >= item.x && mouseY >= item.y && mouseX < item.x + item.width && mouseY < item.y + item.height)
                {
                    drawButton(item.x - 1, item.y - 1, item.x + item.width + 1, item.y + item.height + 1, 0xFFA0A0A0);
                    drawButton(item.x, item.y, item.x + item.width, item.y + item.height, 0xFF8080A0);
                    renderTextFromRight(item.label, item.x + item.width / 2, item.y + (item.height - 8) / 2, 0xFFFFA0);
                    continue;
                }
                drawButton(item.x, item.y, item.x + item.width, item.y + item.height, 0xFF707070);
                renderTextFromRight(item.label, item.x + item.width / 2, item.y + (item.height - 8) / 2, 0xE0E0E0);
            }
        }

        public virtual void keyPressed(char keyChar, Key keyId)
        {
            if (keyId == Key.Escape)
            {
                client.setGui(null);
                client.grabMouse();
            }
        }

        public virtual void btnClicked(MenuItem btn) { }

        public void loadGui(Client client, int width, int height)
        {
            this.client = client;
            screenWidth = width;
            screenHeight = height;
            loadGui();
        }

        public virtual void loadGui() { }

        protected static void drawButton(int left, int top, int right, int bottom, uint color)
        {
            float a = (color >> 24)        / 255.0f;
            float r = (color >> 16 & 0xFF) / 255.0f;
            float g = (color >> 8  & 0xFF) / 255.0f;
            float b = (color       & 0xFF) / 255.0f;
            Tesselator btn = Tesselator.instance;
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Color4(r, g, b, a);
            btn.init();
            btn.vertex(left,  bottom, 0.0f);
            btn.vertex(right, bottom, 0.0f);
            btn.vertex(right, top,    0.0f);
            btn.vertex(left,  top,    0.0f);
            btn.flush();
            GL.Disable(EnableCap.Blend);
        }

        protected static void renderBackground(int left, int top, int right, int bottom, uint color0, uint color1)
        {
            float a0 = (color0 >> 24)        / 255.0f;
            float r0 = (color0 >> 16 & 0xFF) / 255.0f;
            float g0 = (color0 >> 8  & 0xFF) / 255.0f;
            float b0 = (color0       & 0xFF) / 255.0f;
            float a1 = (color1 >> 24)        / 255.0f;
            float r1 = (color1 >> 16 & 0xFF) / 255.0f;
            float g1 = (color1 >> 8  & 0xFF) / 255.0f;
            float b1 = (color1       & 0xFF) / 255.0f;
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(r0, g0, b0, a0);
            GL.Vertex2(right, top);
            GL.Vertex2(left,  top);
            GL.Color4(r1, g1, b1, a1);
            GL.Vertex2(left,  bottom);
            GL.Vertex2(right, bottom);
            GL.End();
            GL.Disable(EnableCap.Blend);
        }

        public void renderTextFromRight(string text, int x, int y, int color)
        {
            Font font = client.font;
            font.drawShadow(text, x - font.width(text) / 2, y, color);
        }

        public void renderText(string text, int x, int y, int color)
        {
            Font font = client.font;
            font.drawShadow(text, x, y, color);
        }

        public void checkInputs()
        {
            int x = (int)Input.MousePos.X * screenWidth / client.width;
            int y = screenHeight - (int)Input.MousePos.Y * screenHeight / client.height - 1;
            bool clicked = Input.MousePress(MouseButton.Left) || Input.MousePress(MouseButton.Right);
            if (clicked)
                for (int i = 0; i < menuItems.Count; i++)
                {
                    MenuItem item = menuItems[i];
                    if (x >= item.x && y >= item.y && x < item.x + item.width && y < item.y + item.height)
                        btnClicked(item);
                }
            Input.UpdateInput();
            object? input = Input.GetKey();
            if ((Key?)input != null)
                keyPressed(input.ToString()![0], (Key)input);
        }

        public virtual void advanceTime() { }
        public virtual void refresh() { }
    }
}
