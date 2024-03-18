using MineCS.mccs.level;
using OpenTK.Graphics.OpenGL;

namespace MineCS.mccs.character
{
    public class Zombie : Entity
    {
        public Cube head;
        public Cube body;
        public Cube arm0;
        public Cube arm1;
        public Cube leg0;
        public Cube leg1;
        public float rot;
        public float timeOffs;
        public float speed;
        public float rotA;

        public Zombie(Level level, float x, float y, float z) : base(level)
        {
            Random rand = new Random();
            this.x = x;
            this.y = y;
            this.z = z;
            timeOffs = rand.NextSingle() * 1239813.0f;
            rot = (float)(rand.NextSingle() * Math.PI * 2.0);
            speed = 1.0f;
            rotA = (float)(rand.NextSingle() + 1.0) * 0.01f;
            head = new Cube(0, 0);
            head.addBox(-4.0f, -8.0f, -4.0f, 8, 8, 8);
            body = new Cube(16, 16);
            body.addBox(-4.0f, 0.0f, -2.0f, 8, 12, 4);
            arm0 = new Cube(40, 16);
            arm0.addBox(-3.0f, -2.0f, -2.0f, 4, 12, 4);
            arm0.setPos(-5.0f, 2.0f, 0.0f);
            arm1 = new Cube(40, 16);
            arm1.addBox(-1.0f, -2.0f, -2.0f, 4, 12, 4);
            arm1.setPos(5.0f, 2.0f, 0.0f);
            leg0 = new Cube(0, 16);
            leg0.addBox(-2.0f, 0.0f, -2.0f, 4, 12, 4);
            leg0.setPos(-2.0f, 12.0f, 0.0f);
            leg1 = new Cube(0, 16);
            leg1.addBox(-2.0f, 0.0f, -2.0f, 4, 12, 4);
            leg1.setPos(2.0f, 12.0f, 0.0f);
        }

        public override void tick()
        {
            Random rand = new Random();
            xo = x;
            yo = y;
            zo = z;
            float xa, ya;
            rot += rotA;
            rotA = (float)(rotA * 0.99);
            rotA = (float)(rotA + (rand.NextSingle() - rand.NextSingle()) * rand.NextSingle() * rand.NextSingle() * 0.01);
            xa = (float)Math.Sin(rot);
            ya = (float)Math.Cos(rot);
            if (onGround && rand.NextSingle() < 0.01)
                yd = 0.12f;
            moveRelative(xa, ya, onGround ? 0.02f : 0.005f);
            yd = (float)(yd - 0.005);
            move(xd, yd, zd);
            xd *= 0.91f;
            yd *= 0.98f;
            zd *= 0.91f;
            if (y > 100.0f)
                resetPos();
            if (onGround)
            {
                xd *= 0.8f;
                zd *= 0.8f;
            }
        }

        public void render(float a)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, Textures.loadTexture("/char.png", 9728));
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
            head.yRot = (float)Math.Sin(time * 0.83);
            head.xRot = (float)Math.Sin(time) * 0.8f;
            arm0.xRot = (float)Math.Sin(time * 0.6662 + Math.PI) * 2.0f;
            arm0.zRot = (float)(Math.Sin(time * 0.2312) + 1.0);
            arm1.xRot = (float)Math.Sin(time * 0.6662) * 2.0f;
            arm1.zRot = (float)(Math.Sin(time * 0.2312) - 1.0);
            leg0.xRot = (float)Math.Sin(time * 0.6662) * 1.4f;
            leg1.xRot = (float)Math.Sin(time * 0.6662 + Math.PI) * 1.4f;
            head.render();
            body.render();
            arm0.render();
            arm1.render();
            leg0.render();
            leg1.render();
            GL.PopMatrix();
            GL.Disable(EnableCap.Texture2D);
        }
    }
}
