using MineCS.rubydung.level;
using MineCS.rubydung.physics;
using OpenTK.Input;

namespace MineCS.rubydung
{
    public class Entity
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
        public float xRot;
        public float yRot;
        public AABB bb;
        public bool onGround = false;
        protected float heightOffset = 0.0f;

        public Entity(Level level)
        {
            this.level = level;
            resetPos();
        }

        public void resetPos()
        {
            Random rnd = new Random();
            float x = rnd.Next(level.width);
            float y = level.depth + 10;
            float z = rnd.Next(level.height);
            setPos(x, y, z);
        }

        private void setPos(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            float w = 0.3f;
            float h = 0.9f;
            bb = new AABB(x - w, y - h, z - w, x + w, y + h, z + w);
        }

        public void turn(float xo, float yo)
        {
            xRot = (float)(xRot + xo * 0.15);
            yRot = (float)(yRot - yo * 0.15);
            if (yRot < -90.0f)
                yRot = -90.0f;
            if (yRot > 90.0f)
                yRot = 90.0f;
        }

        public virtual void tick()
        {
            xo = x;
            yo = y;
            zo = z;
        }

        public void move(float xa, float ya, float za)
        {
            float xaOrg = xa;
            float yaOrg = ya;
            float zaOrg = za;
            List<AABB> aABBs = level.getCubes(bb.expand(xa, ya, za));
            for (int i = 0; i < aABBs.Count(); i++)
                ya = aABBs[i].clipYCollide(bb, ya);
            bb.move(0.0f, ya, 0.0f);
            for (int i = 0; i < aABBs.Count(); i++)
                xa = aABBs[i].clipXCollide(bb, xa);
            bb.move(xa, 0.0f, 0.0f);
            for (int i = 0; i < aABBs.Count(); i++)
                za = aABBs[i].clipZCollide(bb, za);
            bb.move(0.0f, 0.0f, za);
            onGround = yaOrg != ya && yaOrg < 0.0f;
            if (xaOrg != xa)
                xd = 0.0f;
            if (yaOrg != ya)
                yd = 0.0f;
            if (zaOrg != za)
                zd = 0.0f;
            x = (bb.x0 + bb.x1) / 2.0f;
            y = bb.y0 + heightOffset;
            z = (bb.z0 + bb.z1) / 2.0f;
        }

        public void moveRelative(float xa, float za, float speed)
        {
            float dist = xa * xa + za * za;
            if (dist < 0.01f)
                return;
            dist = speed / (float)Math.Sqrt(dist);
            xa *= dist;
            za *= dist;
            float sin = (float)Math.Sin(xRot * Math.PI / 180.0);
            float cos = (float)Math.Cos(xRot * Math.PI / 180.0);
            xd += xa * cos - za * sin;
            zd += za * cos + xa * sin;
        }
    }
}
