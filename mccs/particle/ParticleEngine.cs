using MineCS.mccs.level;
using OpenTK.Graphics.OpenGL;

namespace MineCS.mccs.particle
{
    public class ParticleEngine
    {
        protected Level level;
        private List<Particle> particles = new List<Particle>();

        public ParticleEngine(Level level)
        {
            this.level = level;
        }

        public void add(Particle p)
        {
            particles.Add(p);
        }

        public void tick()
        {
            for (int i = 0; i < particles.Count; i++)
            {
                Particle p = particles[i];
                p.tick();
                if (p.removed)
                    particles.RemoveAt(i--);
            }
        }

        public void render(Player player, float a, int layer)
        {
            GL.Enable(EnableCap.Texture2D);
            int id = Textures.loadTexture("/terrain.png", 9728);
            GL.BindTexture(TextureTarget.Texture2D, id);
            float xa = -(float)Math.Cos(player.yRot * Math.PI / 180.0);
            float za = -(float)Math.Sin(player.yRot * Math.PI / 180.0);
            float xa2 = -za * (float)Math.Sin(player.xRot * Math.PI / 180.0);
            float za2 = xa * (float)Math.Sin(player.xRot * Math.PI / 180.0);
            float ya = (float)Math.Cos(player.xRot * Math.PI / 180.0);
            Tesselator t = Tesselator.instance;
            GL.Color4(0.8f, 0.8f, 0.8f, 1.0f);
            t.init();
            for (int i = 0; i < particles.Count; i++)
            {
                Particle p = particles[i];
                if (p.isLit() ^ layer == 1)
                    p.render(t, a, xa, ya, za, xa2, za2);
            }
            t.flush();
            GL.Disable(EnableCap.Texture2D);
        }
    }
}
