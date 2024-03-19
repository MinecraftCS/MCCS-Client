using MineCS.mccs.level;
using MineCS.mccs.renderer;

namespace MineCS.mccs.particle
{
    public class Particle : Entity
    {
        private float xd2;
        private float yd2;
        private float zd2;
        public int tex;
        public float u0;
        public float v0;
        private int life = 0;
        private int lifetime = 0;
        public float scale;

        public Particle(Level level, float x, float y, float z, float xa, float ya, float za, int tex) : base(level)
        {
            Random rand = new Random();
            this.tex = tex;
            setSize(0.2f, 0.2f);
            heightOffset = bbHeight / 2.0f;
            setPos(x, y, z);
            xd2 = xa + (float)(rand.NextSingle() * 2.0 - 1.0) * 0.4f;
            yd2 = ya + (float)(rand.NextSingle() * 2.0 - 1.0) * 0.4f;
            zd2 = za + (float)(rand.NextSingle() * 2.0 - 1.0) * 0.4f;
            float speed = (float)(rand.NextSingle() + rand.NextSingle() + 1.0) * 0.15f;
            float dd = (float)Math.Sqrt(xd2 * xd2 + yd2 * yd2 + zd2 * zd2);
            xd2 = xd2 / dd * speed * 0.4f;
            yd2 = yd2 / dd * speed * 0.4f + 0.1f;
            zd2 = zd2 / dd * speed * 0.4f;
            u0 = rand.NextSingle() * 3.0f;
            v0 = rand.NextSingle() * 3.0f;
            scale = rand.NextSingle() * 0.5f + 0.5f;
            lifetime = (int)(4.0 / (rand.NextSingle() * 0.9 + 0.1));
            life = 0;
        }

        public override void tick()
        {
            xo = x;
            yo = y;
            zo = z;
            if (life++ >= lifetime)
                remove();
            yd2 = (float)(yd2 - 0.04);
            move(xd2, yd2, zd2);
            xd2 *= 0.98f;
            yd2 *= 0.98f;
            zd2 *= 0.98f;
            if (onGround)
            {
                xd2 *= 0.7f;
                zd2 *= 0.7f;
            }
        }

        public void render(Tesselator t, float a, float xa, float ya, float za, float xa2, float za2)
        {
            float u0 = ((tex % 16) + this.u0 / 4.0f) / 16.0f;
            float u1 = u0 + 0.015609375f;
            float v0 = ((tex / 16) + this.v0 / 4.0f) / 16.0f;
            float v1 = v0 + 0.015609375f;
            float r = 0.1f * scale;
            float x = xo + (this.x - xo) * a;
            float y = yo + (this.y - yo) * a;
            float z = zo + (this.z - zo) * a;
            t.vertexUV(x - xa * r - xa2 * r, y - ya * r, z - za * r - za2 * r, u0, v1);
            t.vertexUV(x - xa * r + xa2 * r, y + ya * r, z - za * r + za2 * r, u0, v0);
            t.vertexUV(x + xa * r + xa2 * r, y + ya * r, z + za * r + za2 * r, u1, v0);
            t.vertexUV(x + xa * r - xa2 * r, y - ya * r, z + za * r - za2 * r, u1, v1);
        }
    }
}
