using MineCS.mccs.level;
using MineCS.mccs.level.tile;
using MineCS.mccs.phys;
using OpenTK.Input;

namespace MineCS.mccs
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
        public bool onWall = false;
        public bool removed = false;
        protected float heightOffset = 0.0f;
        protected float bbWidth = 0.6f;
        protected float bbHeight = 1.8f;

        public Entity(Level level)
        {
            this.level = level;
            resetPos();
        }

        public void resetPos()
        {
            Random rnd = new Random();
            float x = rnd.NextSingle() * (level.width - 2) + 1.0f;
            float y = level.depth + 10;
            float z = rnd.NextSingle() * (level.height - 2) + 1.0f;
            setPos(x, y, z);
        }

        protected void setSize(float w, float h)
        {
            bbWidth = w;
            bbHeight = h;
        }

        protected void setPos(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            float w = bbWidth / 2.0f;
            float h = bbHeight / 2.0f;
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

        public bool aboveThreshold(float x, float y, float z)
        {
            float f5 = z;
            z = y;
            y = x;
            AABB hb = new AABB(bb.x0 + f5, bb.y0 + z, bb.z0 + f5, bb.x1 + y, bb.y1 + z, bb.z1 + f5);
            if (level.getCubes(bb).Count > 0)
                return false;
            int n = (int)Math.Floor(hb.x0);
            int n2 = (int)Math.Floor(hb.x1 + 1.0f);
            int n3 = (int)Math.Floor(hb.y0);
            int n4 = (int)Math.Floor(hb.y1 + 1.0f);
            int n5 = (int)Math.Floor(hb.z0);
            int n6 = (int)Math.Floor(hb.z1 + 1.0f);
            if (n < 0) n = 0;
            if (n3 < 0) n3 = 0;
            if (n5 < 0) n5 = 0;
            if (n2 > level.width) n2 = level.width;
            if (n4 > level.depth) n4 = level.depth;
            if (n6 > level.height) n6 = level.height;
            while (n < n2)
            {
                for (int i = n3; i < n4; ++i)
                {
                    for (int j = n5; j < n6; ++j)
                    {
                        Tile a2 = Tile.tiles[level.getTile(n, i, j)];
                        if (a2 != null && a2.getType() > 0)
                            return false;
                    }
                }
                ++n;
            }
            return true;
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

            onWall = xaOrg != xa || zaOrg != za;
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

        public bool inWater() => level.isTileInRange(bb.grow(0.0f, -0.4f, 0.0f), 1);
        public bool inLava() => level.isTileInRange(bb, 2);
        public void remove() => removed = true;

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

        public bool isLit()
        {
            int xTile = (int)x;
            int yTile = (int)y;
            int zTile = (int)z;
            return level.isLit(xTile, yTile, zTile);
        }

        public virtual void render(float deltaTime) {}
    }
}
