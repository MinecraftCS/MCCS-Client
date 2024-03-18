using MineCS.mccs.level;

namespace MineCS.mccs.particle
{
    public class Particle : Entity
    {
        private float xd;
        private float yd;
        private float zd;
        public int tex;
        private float uo;
        private float vo;

        public Particle(Level level, float x, float y, float z, float xa, float ya, float za, int tex) : base(level)
        {
            Random rand = new Random();
            this.tex = tex;
            setSize(0.2f, 0.2f);
            heightOffset = bbHeight / 2.0f;
            setPos(x, y, z);
            xd = xa + (float)(rand.NextSingle() * 2.0 - 1.0) * 0.4f;
            yd = ya + (float)(rand.NextSingle() * 2.0 - 1.0) * 0.4f;
            zd = za + (float)(rand.NextSingle() * 2.0 - 1.0) * 0.4f;
            float speed = (float)(rand.NextSingle() + rand.NextSingle() + 1.0) * 0.15f;
            float dd = (float)Math.Sqrt(xd * xd + yd * yd + zd * zd);
            xd = xd / dd * speed * 0.7f;
            yd = yd / dd * speed;
            zd = zd / dd * speed * 0.7f;
            uo = rand.NextSingle() * 3.0f;
            vo = rand.NextSingle() * 3.0f;
        }

        public override void tick()
        {
            xo = x;
            yo = y;
            zo = z;
            if (new Random().NextSingle() < 0.1)
                remove();
            yd = (float)(yd - 0.06);
            move(xd, yd, zd);
            xd *= 0.98f;
            yd *= 0.98f;
            zd *= 0.98f;
            if (onGround)
            {
                xd *= 0.7f;
                zd *= 0.7f;
            }
        }

        public void render(Tesselator t, float a, float xa, float ya, float za)
        {
            float u0 = ((tex % 16) + uo / 4.0f) / 16.0f;
            float u1 = u0 + 0.015609375f;
            float v0 = ((tex / 16) + vo / 4.0f) / 16.0f;
            float v1 = v0 + 0.015609375f;
            float r = 0.1f;
            float x = xo + (this.x - xo) * a;
            float y = yo + (this.y - yo) * a;
            float z = zo + (this.z - zo) * a;
            t.vertexUV(x - xa * r, y - ya * r, z - za * r, u0, v1);
            t.vertexUV(x - xa * r, y + ya * r, z - za * r, u0, v0);
            t.vertexUV(x + xa * r, y + ya * r, z + za * r, u1, v0);
            t.vertexUV(x + xa * r, y - ya * r, z + za * r, u1, v1);
        }
    }
}
