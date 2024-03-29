﻿using MineCS.mccs.level;
using MineCS.mccs.renderer;
using OpenTK.Graphics.OpenGL;

namespace MineCS.mccs.character
{
    public class Zombie : Entity
    {
        public float rot;
        public float timeOffs;
        public float speed;
        public float rotA;
        private static ZombieModel zombieModel = new ZombieModel();
        private Textures textures;

        public Zombie(Level level, Textures textures, float x, float y, float z) : base(level)
        {
            Random rand = new Random();
            this.textures = textures;
            setPos(x, y, z);
            timeOffs = rand.NextSingle() * 1239813.0f;  
            rot = (float)(rand.NextSingle() * Math.PI * 2.0);
            speed = 1.0f;
            rotA = (float)(rand.NextSingle() + 1.0) * 0.01f;
        }

        public override void tick()
        {
            base.tick();
            Random rand = new Random();
            float xa, ya;
            if (y < -100.0f)
                remove();
            rot += rotA;
            rotA *= 0.99f;
            rotA += (rand.NextSingle() - rand.NextSingle()) * rand.NextSingle() * rand.NextSingle() * 0.08f;
            xa = (float)Math.Sin(rot);
            ya = (float)Math.Cos(rot);
            if (onGround && rand.NextSingle() < 0.08)
                yd = 0.5f;
            moveRelative(xa, ya, onGround ? 0.1f : 0.02f);
            yd -= 0.08f;
            move(xd, yd, zd);
            xd *= 0.91f;
            yd *= 0.98f;
            zd *= 0.91f;
            if (onGround)
            {
                xd *= 0.7f;
                zd *= 0.7f;
            }
        }

        public override void render(float a)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textures.loadTexture("/char.png", 9728));
            GL.PushMatrix();
            double time = (DateTime.UtcNow.TimeOfDay.TotalMilliseconds * 1000000.0) / 1.0E9 * 10.0 * speed + timeOffs;
            float size = 0.058333334f;
            float yy = (float)(-Math.Abs(Math.Sin(time * 0.6662)) * 5.0 - 23.0);
            GL.Translate(xo + (x - xo) * a, yo + (y - yo) * a, zo + (z - zo) * a);
            GL.Scale(1.0f, -1.0f, 1.0f);
            GL.Scale(size, size, size);
            GL.Translate(0.0f, yy, 0.0f);
            float c = 57.29578f;
            GL.Rotate(rot * c + 180.0f, 0.0f, 1.0f, 0.0f);
            zombieModel.render((float)time);
            GL.PopMatrix();
            GL.Disable(EnableCap.Texture2D);
        }
    }
}
