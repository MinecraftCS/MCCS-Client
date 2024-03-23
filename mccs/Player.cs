using MineCS.mccs.level;
using OpenTK.Input;

namespace MineCS.mccs
{
    public class Player : Entity
    {
        public bool[] inputs = new bool[10];

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

            if (inputs[0])
                ya--;
            if (inputs[1])
                ya++;
            if (inputs[2])
                xa--;
            if (inputs[3])
                xa++;
            if (inputs[4])
            {
                if (inWater)
                    yd += 0.04f;
                else if (inLava)
                    yd += 0.04f;
                else if (onGround)
                {
                    yd = 0.42f;
                    inputs[4] = false;
                }
            }

            if (inWater)
            {
                float oldY = y;
                moveRelative(xa, ya, 0.02f);
                move(xd, yd, zd);
                xd *= 0.8f;
                yd *= 0.8f;
                zd *= 0.8f;
                yd -= 0.02f;
                if (onWall && aboveThreshold(xd, yd + 0.6f - y + oldY, zd))
                    yd = 0.3f;
            }
            else if (inLava)
            {
                float oldY = y;
                moveRelative(xa, ya, 0.02f);
                move(xd, yd, zd);
                xd *= 0.5f;
                yd *= 0.5f;
                zd *= 0.5f;
                yd -= 0.02f;
                if (onWall && aboveThreshold(xd, yd + 0.6f - y + oldY, zd))
                    yd = 0.3f;
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
