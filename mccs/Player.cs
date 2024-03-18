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
                    yd = 0.12f;
            moveRelative(xa, ya, onGround ? 0.02f : 0.005f);
            yd = (float)(yd - 0.005);
            move(xd, yd, zd);
            xd *= 0.91f;
            yd *= 0.98f;
            zd *= 0.91f;
            if (onGround)
            {
                xd *= 0.8f;
                zd *= 0.8f;
            }
        }
    }
}
