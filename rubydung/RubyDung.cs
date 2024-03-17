using MineCS.rubydung.character;
using MineCS.rubydung.level;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Diagnostics;

namespace MineCS.rubydung
{
    public class RubyDung : GameWindow
    {
        private int width;
        private int height;
        private float[] fogColor = new float[4];
        private Timer timer = new Timer(60.0f);
        private Level level;
        private LevelRenderer levelRenderer;
        private Player player;
        private List<Zombie> zombies = new List<Zombie>();
        private int[] viewportBuffer = new int[16];
        private int[] selectBuffer = new int[2000];
        private int selectBufferIndex = 0;
        private HitResult hitResult = null;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Width = 1024;
            Height = 768;
            VSync = VSyncMode.Off;
            Title = "Cave Game";
            init();
        }

        public void init() 
        {
            int col = 0x0E0B0A;
            float fr = 0.5f;
            float fg = 0.8f;
            float fb = 1.0f;
            fogColor = new float[] 
            { 
                (col >> 16 & 0xFF) / 255.0f, 
                (col >> 8  & 0xFF) / 255.0f, 
                (col       & 0xFF) / 255.0f, 
                1.0f
            };
            GL.Viewport(0, 0, Width, Height);
            width = Width;
            height = Height;
            GL.Enable(EnableCap.Texture2D);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.ClearColor(fr, fg, fb, 0.0f);
            GL.ClearDepth(1.0);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Modelview);
            level = new Level(256, 256, 64);
            levelRenderer = new LevelRenderer(level);
            player = new Player(level);
            Input.Initialize(this);
            CursorVisible = false;
            for (int i = 0; i < 100; i++)
                zombies.Add(new Zombie(level, 128.0f, 0.0f, 128.0f));
        }

        public void destroy()
        {
            level.save();
            Close();
        }

        private static long lastTime = (long)DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
        private static int frames;
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            if (!Input.KeyDown(Key.Escape))
            {
                timer.advanceTime();
                for (int i = 0; i < timer.ticks; i++)
                    tick();
                render(timer.a);
                frames++;
                if (DateTime.UtcNow.TimeOfDay.TotalMilliseconds >= lastTime + 1000)
                {
                    Debug.WriteLine($"{frames} fps, {Chunk.updates}");
                    Chunk.updates = 0;
                    lastTime += 1000L;
                    frames = 0;
                }
            }
            else
                Close();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            width = Width;
            height = Height;
            base.OnResize(e);
        }

        public void tick()
        {
            for (int i = 0; i < zombies.Count; i++)
                zombies[i].tick();
            player.tick();
            Input.UpdateInput();
        }

        private void moveCameraToPlayer(float a)
        {
            GL.Translate(0.0f, 0.0f, -0.3f);
            GL.Rotate(player.yRot, 1.0f, 0.0f, 0.0f);
            GL.Rotate(player.xRot, 0.0f, 1.0f, 0.0f);
            float x = player.xo + (player.x - player.xo) * a;
            float y = player.yo + (player.y - player.yo) * a;
            float z = player.zo + (player.z - player.zo) * a;
            GL.Translate(-x, -y, -z);
        }

        private void setupCamera(float a)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Glu.Perspective(70.0f, (float)width / height, 0.05f, 1000.0f);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            moveCameraToPlayer(a);
        }

        private void setupPickCamera(float a, int x, int y)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Array.Clear(viewportBuffer);
            GL.GetInteger(GetIndexedPName.Viewport, 0, viewportBuffer);
            Array.Resize(ref viewportBuffer, 16);
            Glu.PickMatrix(x, y, 5.0f, 5.0f, viewportBuffer);
            Glu.Perspective(70.0f, (float)width / height, 0.05f, 1000.0f);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            moveCameraToPlayer(a);
        }

        private void pick(float a)
        {
            selectBufferIndex = 0;
            GL.SelectBuffer(selectBuffer.Length, selectBuffer);
            GL.RenderMode(RenderingMode.Select);
            setupPickCamera(a, width / 2, height / 2);
            levelRenderer.pick(player);
            int hits = GL.RenderMode(RenderingMode.Render);
            selectBufferIndex = 0;
            Array.Resize(ref selectBuffer, selectBuffer.Length);
            long closest = 0L;
            int[] names = new int[10];
            int hitNameCount = 0;
            for (int i = 0; i < hits; i++)
            {
                int nameCount = selectBuffer[selectBufferIndex++];
                long minZ = selectBuffer[selectBufferIndex++];
                selectBufferIndex++;
                long dist = minZ;
                if (dist < closest || i == 0)
                {
                    closest = dist;
                    hitNameCount = nameCount;
                    for (int j = 0; j < nameCount; j++)
                        names[j] = selectBuffer[selectBufferIndex++];
                }
                else
                {
                    for (int j = 0; j < nameCount; j++)
                        selectBufferIndex++;
                }
            }
            hitResult = hitNameCount > 0 ? new HitResult(names[0], names[1], names[2], names[3], names[4]) : null!;
        }

        public void render(float a)
        {
            float xo = Input.MouseDelta.X;
            float yo = -Input.MouseDelta.Y;
            player.turn(xo, yo);
            pick(a);

            if (Input.MousePress(MouseButton.Right) && hitResult != null)
                level.setTile(hitResult.x, hitResult.y, hitResult.z, 0);
            if (Input.MousePress(MouseButton.Left) && hitResult != null)
            {
                int x = hitResult.x;
                int y = hitResult.y;
                int z = hitResult.z;
                if (hitResult.f == 0)
                    y--;
                if (hitResult.f == 1)
                    y++;
                if (hitResult.f == 2)
                    z--;
                if (hitResult.f == 3)
                    z++;
                if (hitResult.f == 4)
                    x--;
                if (hitResult.f == 5)
                    x++;
                level.setTile(x, y, z, 1);
            }
            if (Input.KeyPress(Key.Enter))
                level.save();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            setupCamera(a);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Fog);
            GL.Fog(FogParameter.FogMode, 2048);
            GL.Fog(FogParameter.FogDensity, 0.2f);
            GL.Fog(FogParameter.FogColor, fogColor);
            GL.Disable(EnableCap.Fog);
            levelRenderer.render(player, 0);
            for (int i = 0; i < zombies.Count; i++)
                zombies[i].render(a);
            GL.Enable(EnableCap.Fog);
            levelRenderer.render(player, 1);
            GL.Disable(EnableCap.Texture2D);
            if (hitResult != null)
                levelRenderer.renderHit(hitResult);
            GL.Disable(EnableCap.Fog);
            Context.SwapBuffers();
            Input.UpdateCursor();
        }

        public static void checkError()
        {
            var e = GL.GetError();
            if (e != ErrorCode.NoError)
                throw new Exception(e.ToString());
        }
    }
}
