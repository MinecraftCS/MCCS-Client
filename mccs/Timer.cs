namespace MineCS.mccs
{
    public class Timer
    {
        private static long NS_PER_SECOND = 1000000000L;
        private static long MAX_NS_PER_UPDATE = 1000000000L;
        private static int MAX_TICKS_PER_UPDATE = 100;

        private float ticksPerSecond;
        private long lastTime;
        public int ticks;
        public float a;
        public float timeScale = 1.0f;
        public float fps = 0.0f;
        public float passedTime = 0.0f;

        public Timer(float ticksPerSecond)
        {
            this.ticksPerSecond = ticksPerSecond;
            lastTime = (long)(DateTime.UtcNow.TimeOfDay.TotalMilliseconds * 1000000.0);
        }

        public void advanceTime()
        {
            long now = (long)(DateTime.UtcNow.TimeOfDay.TotalMilliseconds * 1000000.0);
            long passedNs = now - lastTime;
            lastTime = now;
            if (passedNs < 0L)
                passedNs = 0L;
            if (passedNs > MAX_NS_PER_UPDATE)
                passedNs = MAX_NS_PER_UPDATE;
            fps = NS_PER_SECOND / passedNs;
            passedTime += passedNs * timeScale * ticksPerSecond / MAX_NS_PER_UPDATE;
            ticks = (int)passedTime;
            if (ticks > MAX_TICKS_PER_UPDATE)
                ticks = MAX_TICKS_PER_UPDATE;
            passedTime -= ticks;
            a = passedTime;
        }
    }
}
