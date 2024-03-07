using MineCS.rubydung.level;
using MineCS.rubydung.physics;
using OpenTK.Input;

namespace MineCS.rubydung
{
    public class Player
    {
        private Level level;

        public float xo;

        public float yo;

        public float zo;

        public float x;

        public float y;

        public float z;

        public float xd;

        public float yd;

        public float zd;

        public float yRot;

        public float xRot;

        public AABB bb;

        public bool onGround = false;

        public Player(Level level)
        {
            this.level = level;
            resetPos();
        }

        private void resetPos()
        {
            Random rnd = new Random();
            float x = rnd.Next(level.width);
            float y = level.depth+10;
            float z = rnd.Next(level.height);
            setPos(x, y, z);
        }

        private void setPos(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            float w = 0.3F;
            float h = 0.9F;
            bb = new AABB(x - w, y - h, z - w, x + w, y + h, z + w);
        }

        public void turn(float xo, float yo)
        {
            yRot = (float)(yRot + xo * 0.15D);
            xRot = (float)(xRot - yo * 0.15D);
            if (xRot < -90.0F)
                xRot = -90.0F;
            if (xRot > 90.0F)
                xRot = 90.0F;
        }

        public void tick()
        {
            xo = x;
            yo = y;
            zo = z;
            float xa = 0.0F;
            float ya = 0.0F;

            if (Input.KeyDown(Key.R))
                resetPos();
            if (Input.KeyDown(Key.Up) || Input.KeyDown(Key.W))
                ya--;
            if (Input.KeyDown(Key.Down) || Input.KeyDown(Key.S))
                ya++;
            if (Input.KeyDown(Key.Left) || Input.KeyDown(Key.A))
                xa--;
            if (Input.KeyDown(Key.Right) || Input.KeyDown(Key.D))
                xa++;
            if (Input.KeyDown(Key.Space) || Input.KeyDown(Key.LAlt))
                if (onGround)
                    yd = 0.12F;
            moveRelative(xa, ya, onGround ? 0.02F : 0.005F);
            yd = (float)(yd - 0.005D);
            move(xd, yd, zd);
            xd *= 0.91F;
            yd *= 0.98F;
            zd *= 0.91F;
            if (onGround)
            {
                xd *= 0.8F;
                zd *= 0.8F;
            }

            /*Console.Clear();
            Console.WriteLine("Player Information:");
            Console.WriteLine($"XYZ Position: {x}, {y}, {z}");
            Console.WriteLine($"XY Rotation: {xRot}, {yRot}");
            Console.WriteLine($"XYZ Delta: {xd}, {yd}, {zd}");
            Console.WriteLine($"On Ground: {onGround}");*/
        }

        public void move(float xa, float ya, float za)
        {
            float xaOrg = xa;
            float yaOrg = ya;
            float zaOrg = za;
            List<AABB> aABBs = level.getCubes(bb.expand(xa, ya, za));
            int i;
            for (i = 0; i < aABBs.Count(); i++)
                ya = ((AABB)aABBs[i]).clipYCollide(bb, ya);
            bb.move(0.0F, ya, 0.0F);
            for (i = 0; i < aABBs.Count(); i++)
                xa = ((AABB)aABBs[i]).clipXCollide(bb, xa);
            bb.move(xa, 0.0F, 0.0F);
            for (i = 0; i < aABBs.Count(); i++)
                za = ((AABB)aABBs[i]).clipZCollide(bb, za);
            bb.move(0.0F, 0.0F, za);
            onGround = (yaOrg != ya && yaOrg < 0.0F);
            if (xaOrg != xa)
                xd = 0.0F;
            if (yaOrg != ya)
                yd = 0.0F;
            if (zaOrg != za)
                zd = 0.0F;
            x = (bb.x0 + bb.x1) / 2.0F;
            y = bb.y0 + 1.62F;
            z = (bb.z0 + bb.z1) / 2.0F;
        }

        public void moveRelative(float xa, float za, float speed)
        {
            float dist = xa * xa + za * za;
            if (dist < 0.01F)
                return;
            dist = speed / (float)Math.Sqrt(dist);
            xa *= dist;
            za *= dist;
            float sin = (float)Math.Sin(yRot * Math.PI / 180.0D);
            float cos = (float)Math.Cos(yRot * Math.PI / 180.0D);
            xd += xa * cos - za * sin;
            zd += za * cos + xa * sin;
        }
    }
}
