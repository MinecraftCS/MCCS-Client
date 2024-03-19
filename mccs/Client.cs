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

namespace MineCS.mccs
{
    public class Client : GameWindow
    {
        public const string VERSION_STRING = "0.0.12a_03";

        private bool fullscreen = false;
        public int width;
        public int height;
        private float[] fogColor0 = new float[4];
        private float[] fogColor1 = new float[4];
        private Timer timer = new Timer(20.0f);
        private Level level;
        private LevelRenderer levelRenderer;
        private Player player;
        private int paintTexture = 1;
        private ParticleEngine particleEngine;
        private List<Entity> entities = new();
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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            init();
        }

        public void init() 
        {
            float fr = 0.5f;
            float fg = 0.8f;
            float fb = 1.0f;
            fogColor0 = new float[] 
            { 
                fr, fg, fb, 1.0f
            };
            fogColor1 = new float[] 
            { 
                14 / 255.0f,
                11 / 255.0f,
                10 / 255.0f,
                1.0f
            };

            width = Width;
            height = Height;
            Title = "MCCS " + VERSION_STRING;
            checkGlError("Pre startup");
            GL.Viewport(0, 0, Width, Height);
            GL.Enable(EnableCap.Texture2D);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.ClearColor(fr, fg, fb, 0.0f);
            GL.ClearDepth(1.0);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, 0.0f);
            GL.CullFace(CullFaceMode.Back);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Modelview);
            checkGlError("Startup");
            font = new Font("/default.gif", textures);
            GL.Viewport(0, 0, width, height);
            level = new Level(this, 256, 256, 64);
            levelRenderer = new LevelRenderer(level, textures);
            player = new Player(level);
            particleEngine = new ParticleEngine(textures);
            for (int i = 0; i < 10; i++)
            {
                Zombie zombie = new Zombie(level, textures, 128.0f, 0.0f, 128.0f);
                zombie.resetPos();
                entities.Add(zombie);
            }
            Input.Initialize(this);
            CursorVisible = false;
            mouseGrabbed = true;
            running = true;
            checkGlError("Post startup");
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
                render(timer.deltaTime);
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
                AABB aabb = Tile.tiles[paintTexture].getAABB(x, y, z);
                if (aabb == null || isFree(aabb))
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
            if (Input.KeyPress(Key.N))
            {
                level.createLevel();
                player.resetPos();
                entities.Clear();
            }
            if (Input.KeyPress(Key.F))
                levelRenderer.renderDistance = ++levelRenderer.renderDistance % 4;

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

        private void moveCameraToPlayer(float deltaTime)
        {
            GL.Translate(0.0f, 0.0f, -0.3f);
            GL.Rotate(player.yRot, 1.0f, 0.0f, 0.0f);
            GL.Rotate(player.xRot, 0.0f, 1.0f, 0.0f);
            float x = player.xo + (player.x - player.xo) * deltaTime;
            float y = player.yo + (player.y - player.yo) * deltaTime;
            float z = player.zo + (player.z - player.zo) * deltaTime;
            GL.Translate(-x, -y, -z);
        }

        private void setupCamera(float deltaTime)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Glu.Perspective(70.0f, (float)width / height, 0.05f, 1000.0f);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            moveCameraToPlayer(deltaTime);
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

        private void setupPickCamera(float deltaTime, int x, int y)
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
            moveCameraToPlayer(deltaTime);
        }

        private void pick(float deltaTime)
        {
            selectBufferIndex = 0;
            GL.SelectBuffer(selectBuffer.Length, selectBuffer);
            GL.RenderMode(RenderingMode.Select);
            setupPickCamera(deltaTime, width / 2, height / 2);
            levelRenderer.pick(player, Frustum.getFrustum());
            int hits = GL.RenderMode(RenderingMode.Render);
            selectBufferIndex = 0;
            int[] names = new int[10];
            hitResult = null;
            for (int i = 0; i < hits; i++)
            {
                int nameCount = selectBuffer[selectBufferIndex++];
                selectBufferIndex += 2;
                for (int j = 0; j < nameCount; j++)
                    names[j] = selectBuffer[selectBufferIndex++];
                HitResult hR = new HitResult(names[0], names[1], names[2], names[3], names[4]);
                if (hitResult != null && hR.distanceFromBlock(player, editMode) >= hitResult.distanceFromBlock(player, editMode))
                    continue;
                hitResult = hR;
            }
        }

        public void render(float deltaTime)
        {
            if (!Focused)
                releaseMouse();
            GL.Viewport(0, 0, width, height);
            if (mouseGrabbed)
            {
                float xo = Input.MouseDelta.X;
                float yo = -Input.MouseDelta.Y;
                player.turn(xo, yo * yMouseAxis);
            }
            checkGlError("Set viewport");
            pick(deltaTime);
            checkGlError("Picked");
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            setupCamera(deltaTime);
            checkGlError("Set up camera");
            GL.Enable(EnableCap.CullFace);
            Frustum frustum = Frustum.getFrustum();
            levelRenderer.updateDirtyChunks(player, frustum);
            checkGlError("Update chunks");
            setupFog(0);
            GL.Enable(EnableCap.Fog);
            levelRenderer.render(player, 0);
            checkGlError("Rendered level");
            for (int i = 0; i < entities.Count; i++)
                if (entities[i].isLit() && frustum.cubeInFrustum(entities[i].bb))
                    entities[i].render(deltaTime);
            checkGlError("Rendered entities");
            particleEngine.render(player, deltaTime, 0);
            checkGlError("Rendered particles");
            setupFog(1);
            levelRenderer.render(player, 1);
            for (int i = 0; i < entities.Count; i++)
                if (!entities[i].isLit() && frustum.cubeInFrustum(entities[i].bb))
                    entities[i].render(deltaTime);
            particleEngine.render(player, deltaTime, 1);
            GL.CallList(levelRenderer.listIndex);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            setupFog(0);
            GL.CallList(levelRenderer.listIndex + 1);
            GL.Enable(EnableCap.Blend);
            GL.DepthMask(false);
            levelRenderer.render(player, 2);
            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Fog);
            checkGlError("Rendered rest");
            if (hitResult != null)
            {
                GL.Disable(EnableCap.AlphaTest);
                levelRenderer.renderHit(player, hitResult, editMode, paintTexture);
                GL.Enable(EnableCap.AlphaTest);
            }
            checkGlError("Rendered hit");
            drawGui(deltaTime);
            checkGlError("Rendered gui");
            Context.SwapBuffers();
            Input.UpdateCursor();
        }

        private void drawGui(float deltaTime)
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
            t.vertex(wc, hc - 4, 0.0f);
            t.vertex(wc, hc + 5, 0.0f);
            t.vertex(wc + 1, hc + 5, 0.0f);
            t.vertex(wc + 5, hc, 0.0f);
            t.vertex(wc - 4, hc, 0.0f);
            t.vertex(wc - 4, hc + 1, 0.0f);
            t.vertex(wc + 5, hc + 1, 0.0f);
            t.flush();
            checkGlError("GUI: Draw crosshair");
        }

        private void setupFog(int i)
        {
            Tile tile = Tile.tiles[level.getTile((int)player.x, (int)player.y, (int)player.z)];
            if (tile != null && tile.getType() == 1)
            {
                GL.Fog(FogParameter.FogMode, 2048);
                GL.Fog(FogParameter.FogDensity, 0.1f);
                GL.Fog(FogParameter.FogColor, getBuffer(0.02f, 0.02f, 0.2f, 1.0f));
                GL.LightModel(LightModelParameter.LightModelAmbient, getBuffer(0.3f, 0.3f, 0.5f, 1.0f));
            }
            else if (tile != null && tile.getType() == 2)
            {
                GL.Fog(FogParameter.FogMode, 2048);
                GL.Fog(FogParameter.FogDensity, 0.2f);
                GL.Fog(FogParameter.FogColor, getBuffer(0.5f, 0.3f, 0.0f, 1.0f));
                GL.LightModel(LightModelParameter.LightModelAmbient, getBuffer(0.4f, 0.3f, 0.3f, 1.0f));
            }
            else if (i == 0)
            {
                GL.Fog(FogParameter.FogMode, 2048);
                GL.Fog(FogParameter.FogDensity, 0.001f);
                GL.Fog(FogParameter.FogColor, fogColor0);
                GL.LightModel(LightModelParameter.LightModelAmbient, getBuffer(1.0f, 1.0f, 1.0f, 1.0f));
            }
            else if (i == 1)
            {
                GL.Fog(FogParameter.FogMode, 2048);
                GL.Fog(FogParameter.FogDensity, 0.01f);
                GL.Fog(FogParameter.FogColor, fogColor1);
                GL.LightModel(LightModelParameter.LightModelAmbient, getBuffer(0.6f, 0.6f, 0.6f, 1.0f));
            }
            GL.Enable(EnableCap.ColorMaterial);
            GL.ColorMaterial(MaterialFace.Front, ColorMaterialParameter.Ambient);
            GL.Enable(EnableCap.Lighting);
        }

        private float[] getBuffer(float a, float b, float c, float d)
        {
            return new float[] { a, b, c, d };
        }

        public void loadingScreen(string message, string subMessage)
        {
            int screenWidth = width * 240 / height;
            int screenHeight = height * 240 / height;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Tesselator t = Tesselator.instance;
            GL.Enable(EnableCap.Texture2D);
            int id = textures.loadTexture("/dirt.png", 9728);
            GL.BindTexture(TextureTarget.Texture2D, id);
            t.init();
            t.color(0x808080);
            t.vertexUV(0.0f, screenHeight, 0.0f, 0.0f, screenHeight / 32.0f);
            t.vertexUV(screenWidth, screenHeight, 0.0f, screenWidth / 32.0f, screenHeight / 32.0f);
            t.vertexUV(screenWidth, 0.0f, 0.0f, screenWidth / 32.0f, 0.0f);
            t.vertexUV(0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
            t.flush();
            GL.Enable(EnableCap.Texture2D);
            font.drawShadow(message, (screenWidth - font.width(message)) / 2, screenHeight / 2 + 4, 0xFFFFFF);
            font.drawShadow(subMessage, (screenWidth - font.width(subMessage)) / 2, screenHeight / 2 - 8, 0xFFFFFF);
            Context.SwapBuffers();
        }
    }
}
