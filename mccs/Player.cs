using MineCS.mccs.level;
using OpenTK.Input;

namespace MineCS.mccs
{
    public class Player : Entity
    {
        public Player(Level level) : base(level)
        {
            heightOffset = 1.62f;
        }

        public override void tick()
        {
            base.tick();
            float xa = 0.0f;
            float ya = 0.0f;
            bool inWater = base.inWater();
            bool inLava = base.inLava();

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
            {
                if (inWater)
                    yd += 0.06f;
                else if (inLava)
                    yd += 0.04f;
                else if (onGround)
                    yd = 0.5f;
            }

            if (inWater)
            {
                moveRelative(xa, ya, 0.02f);
                move(xd, yd, zd);
                xd *= 0.7f;
                yd *= 0.7f;
                zd *= 0.7f;
                yd -= 0.02f;
            }
            else if (inLava)
            {
                moveRelative(xa, ya, 0.02f);
                move(xd, yd, zd);
                xd *= 0.5f;
                yd *= 0.5f;
                zd *= 0.5f;
                yd -= 0.02f;
            }
            else
            {
                moveRelative(xa, ya, onGround ? 0.1f : 0.02f);
                move(xd, yd, zd);
                xd *= 0.91f;
                yd *= 0.98f;
                zd *= 0.91f;
                yd -= 0.08f;
                if (onGround)
                {
                    xd *= 0.6f;
                    zd *= 0.6f;
                }
            }
        }
    }
}
