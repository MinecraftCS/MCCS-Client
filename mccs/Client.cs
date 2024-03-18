using MineCS.mccs.character;
using MineCS.mccs.gui;
using MineCS.mccs.level;
using MineCS.mccs.level.tile;
using MineCS.mccs.particle;
using MineCS.mccs.phys;
using MineCS.mccs.renderer;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.ComponentModel;
using System.Diagnostics;

namespace MineCS.mccs
{
    public class Client : GameWindow
    {
        public static string VERSION_STRING = "0.0.11a";
        private bool fullscreen = false;
        private int width;
        private int height;
        private float[] fogColor0 = new float[4];
        private float[] fogColor1 = new float[4];
        private Timer timer = new Timer(20.0f);
        private Level level;
        private LevelRenderer levelRenderer;
        private Player player;
        private int paintTexture = 1;
        private ParticleEngine particleEngine; private List<Entity> entities = new();
        public bool appletMode = false;
        public volatile bool pause = false;
        private int yMouseAxis = 1;
        public Textures textures = new Textures();
        private Font font;
        private int editMode = 0;
        private volatile bool running = false;
        private string fpsString = string.Empty;
        public static bool mouseGrabbed = false;
        private int[] viewportBuffer = new int[16];
        private int[] selectBuffer = new int[2000];
        private int selectBufferIndex = 0;
        private HitResult hitResult = null;

        public Client(int width, int height, bool fullscreen) : base(width, height, new GraphicsMode(), "MCCS", fullscreen ? GameWindowFlags.Fullscreen : GameWindowFlags.Default)
        {
            this.fullscreen = fullscreen;
            VSync = VSyncMode.Off;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            init();
        }

        public void init() 
        {
            int col0 = 0xFEFBFA;
            int col1 = 0x0E0B0A;
            float fr = 0.5f;
            float fg = 0.8f;
            float fb = 1.0f;
            fogColor0 = new float[] 
            { 
                (col0 >> 16 & 0xFF) / 255.0f, 
                (col0 >> 8  & 0xFF) / 255.0f, 
                (col0       & 0xFF) / 255.0f, 
                1.0f
            };
            fogColor1 = new float[] 
            { 
                (col1 >> 16 & 0xFF) / 255.0f, 
                (col1 >> 8  & 0xFF) / 255.0f, 
                (col1       & 0xFF) / 255.0f, 
                1.0f
            };

            if (fullscreen)
            {
                width = DisplayDevice.Default.Width;
                height = DisplayDevice.Default.Height;
            }
            Title = "MCCS " + VERSION_STRING;
            checkGlError("Pre startup");
            GL.Viewport(0, 0, Width, Height);
            width = Width;
            height = Height;
            GL.Enable(EnableCap.Texture2D);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.ClearColor(fr, fg, fb, 0.0f);
            GL.ClearDepth(1.0);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, 0.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Modelview);
            checkGlError("Startup");
            level = new Level(256, 256, 64);
            levelRenderer = new LevelRenderer(level, textures);
            player = new Player(level);
            particleEngine = new ParticleEngine(level, textures);
            font = new Font("/default.gif", textures);
            Input.Initialize(this);
            CursorVisible = false;
            running = true;
            mouseGrabbed = true;
            for (int i = 0; i < 10; i++)
            {
                Zombie zombie = new Zombie(level, textures, 128.0f, 0.0f, 128.0f);
                zombie.resetPos();
                entities.Add(zombie);
            }
            checkGlError("Post startup");
        }

        private void checkGlError(string str)
        {
            ErrorCode e = GL.GetError();
            if (e != ErrorCode.NoError)
            {
                string errorString = "########## GL ERROR ##########" + '\n';
                errorString += "@ " + str + '\n';
                errorString += (int)e + ": " + Glu.ErrorString(e);
                throw new SystemException(errorString);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            level.save();
        }

        private static long lastTime = (long)DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
        private static int frames;
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            if (running)
            {
                if (pause) return;
                timer.advanceTime();
                for (int i = 0; i < timer.ticks; i++)
                    tick();
                checkGlError("Pre render");
                render(timer.a);
                checkGlError("Post render");
                frames++;
                if (DateTime.UtcNow.TimeOfDay.TotalMilliseconds >= lastTime + 1000)
                {
                    fpsString = $"{frames} fps, {Chunk.updates} chunk updates";
                    Chunk.updates = 0;
                    lastTime += 1000L;
                    frames = 0;
                }
            }
            else
                Close();
        }

        public void stop()
        {
            running = false;
        }

        public void grabMouse()
        {
            mouseGrabbed = true;
        }

        public void releaseMouse()
        {
            mouseGrabbed = false;
        }

        private void handleMouseClick()
        {
            if (editMode == 0 && hitResult != null)
            {
                Tile oldTile = Tile.tiles[level.getTile(hitResult.x, hitResult.y, hitResult.z)];
                bool changed = level.setTile(hitResult.x, hitResult.y, hitResult.z, 0);
                if (oldTile != null && changed)
                    oldTile.destroy(level, hitResult.x, hitResult.y, hitResult.z, particleEngine);
            }
            else if (hitResult != null)
            {
                AABB aabb;
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
                if ((aabb = Tile.tiles[paintTexture].getAABB(x, y, z)) == null || isFree(aabb))
                    level.setTile(x, y, z, paintTexture);
            }
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
            if (!mouseGrabbed && Input.MousePress(MouseButton.Left))
                grabMouse();
            else
            {
                if (Input.MousePress(MouseButton.Left))
                    handleMouseClick();
                else if (Input.MousePress(MouseButton.Right))
                    editMode = ++editMode % 2;
            }

            if (Input.KeyPress(Key.Escape) && !fullscreen)
                releaseMouse();
            if (Input.KeyPress(Key.Enter))
                level.save();
            if (Input.KeyPress(Key.Number1))
                paintTexture = 1;
            if (Input.KeyPress(Key.Number2))
                paintTexture = 3;
            if (Input.KeyPress(Key.Number3))
                paintTexture = 4;
            if (Input.KeyPress(Key.Number4))
                paintTexture = 5;
            if (Input.KeyPress(Key.Number6))
                paintTexture = 6;
            if (Input.KeyPress(Key.Y))
                yMouseAxis *= -1;
            if (Input.KeyPress(Key.G))
                entities.Add(new Zombie(level, textures, player.x, player.y, player.z));

            level.tick();
            particleEngine.tick();
            for (int i = 0; i < entities.Count; i++)
            {
                entities[i].tick();
                if (entities[i].removed)
                    entities.RemoveAt(i--);
            }
            player.tick();
            Input.UpdateInput();
        }

        private bool isFree(AABB aabb)
        {
            if (player.bb.intersects(aabb))
                return false;
            for (int i = 0; i < entities.Count; i++)
                if (entities[i].bb.intersects(aabb))
                    return false;
            return true;
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

        private void setupOrthoCamera(int screenWidth, int screenHeight)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0.0, screenWidth, screenHeight, 0.0, 100.0, 300.0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Translate(0.0f, 0.0f, -200.0f);
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
            levelRenderer.pick(player, Frustum.getFrustum());
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
            if (!Focused)
                releaseMouse();
            GL.Viewport(0, 0, width, height);
            if (mouseGrabbed)
            {
                float xo = Input.MouseDelta.X;
                float yo = -Input.MouseDelta.Y;
                player.turn(xo, yo);
            }
            checkGlError("Set viewport");
            pick(a);
            checkGlError("Picked");
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            setupCamera(a);
            checkGlError("Set up camera");
            GL.Enable(EnableCap.CullFace);
            Frustum frustum = Frustum.getFrustum();
            levelRenderer.updateDirtyChunks(player);
            checkGlError("Update chunks");
            setupFog(0);
            GL.Enable(EnableCap.Fog);
            levelRenderer.render(player, 0);
            checkGlError("Rendered level");
            for (int i = 0; i < entities.Count; i++)
                if (entities[i].isLit() && frustum.cubeInFrustum(entities[i].bb))
                    entities[i].render(a);
            checkGlError("Rendered entities");
            particleEngine.render(player, a, 0);
            checkGlError("Rendered particles");
            setupFog(1);
            levelRenderer.render(player, 1);
            for (int i = 0; i < entities.Count; i++)
                if (!entities[i].isLit() && frustum.cubeInFrustum(entities[i].bb))
                    entities[i].render(a);
            particleEngine.render(player, a, 1);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Fog);
            checkGlError("Rendered rest");
            if (hitResult != null)
            {
                GL.Disable(EnableCap.AlphaTest);
                levelRenderer.renderHit(hitResult, editMode, paintTexture);
                GL.Enable(EnableCap.AlphaTest);
            }
            checkGlError("Rendered hit");
            drawGui(a);
            checkGlError("Rendered gui");
            Context.SwapBuffers();
            Input.UpdateCursor();
        }

        private void drawGui(float a)
        {
            int screenWidth = width * 240 / height;
            int screenHeight = height * 240 / height;
            GL.Clear(ClearBufferMask.DepthBufferBit);
            setupOrthoCamera(screenWidth, screenHeight);
            checkGlError("GUI: Init");
            GL.PushMatrix();
            GL.Translate(screenWidth - 16.0f, 16.0f, 0.0f);
            Tesselator t = Tesselator.instance;
            GL.Scale(16.0f, 16.0f, 16.0f);
            GL.Rotate(30.0f, 1.0f, 0.0f, 0.0f);
            GL.Rotate(45.0f, 0.0f, 1.0f, 0.0f);
            GL.Translate(-1.5f, 0.5f, -0.5f);
            GL.Scale(-1.0f, -1.0f, 1.0);
            int id = textures.loadTexture("/terrain.png", 9728);
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.Enable(EnableCap.Texture2D);
            t.init();
            Tile.tiles[paintTexture].render(t, level, 0, -2, 0, 0);
            t.flush();
            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();
            checkGlError("GUI: Draw selected");
            font.drawShadow(VERSION_STRING, 2, 2, 0xFFFFFF);
            font.drawShadow(fpsString, 2, 12, 0xFFFFFF);
            checkGlError("GUI: Draw text");
            int wc = screenWidth / 2;
            int hc = screenHeight / 2;
            GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
            t.init();
            t.vertex(wc + 1, hc - 4, 0.0f);
            t.vertex(wc,     hc - 4, 0.0f);
            t.vertex(wc,     hc + 5, 0.0f);
            t.vertex(wc + 1, hc + 5, 0.0f);
            t.vertex(wc + 5, hc,     0.0f);
            t.vertex(wc - 4, hc,     0.0f);
            t.vertex(wc - 4, hc + 1, 0.0f);
            t.vertex(wc + 5, hc + 1, 0.0f);
            t.flush();
            checkGlError("GUI: Draw crosshair");
        }

        private void setupFog(int i)
        {
            if (i == 0)
            {
                GL.Fog(FogParameter.FogMode, 2048);
                GL.Fog(FogParameter.FogDensity, 0.001f);
                GL.Fog(FogParameter.FogColor, fogColor0);
                GL.Disable(EnableCap.Lighting);
            }
            else if (i == 1)
            {
                GL.Fog(FogParameter.FogMode, 2048);
                GL.Fog(FogParameter.FogDensity, 0.06f);
                GL.Fog(FogParameter.FogColor, fogColor1);
                GL.Enable(EnableCap.Lighting);
                GL.Enable(EnableCap.ColorMaterial);
                float br = 0.6f;
                GL.LightModel(LightModelParameter.LightModelAmbient, getBuffer(br, br, br, 1.0f));
            }
        }

        private float[] getBuffer(float a, float b, float c, float d)
        {
            return new float[] { a, b, c, d };
        }

        public static void checkError()
        {
            ErrorCode e = GL.GetError();
            if (e != ErrorCode.NoError)
                throw new Exception(Glu.ErrorString(e));
        }
    }
}
