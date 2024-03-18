using MineCS.mccs.renderer;

namespace MineCS.mccs.level
{
    public class DirtyChunkSorter : IComparer<Chunk>
    {
        private Player player;
        private Frustum frustum;
        private long now = (long)DateTime.UtcNow.TimeOfDay.TotalMilliseconds;

        public DirtyChunkSorter(Player player, Frustum frustum)
        {
            this.player = player;
            this.frustum = frustum;
        }

        public int Compare(Chunk? c0, Chunk? c1)
        {
            if (c0 == null || c1 == null) return -1;
            bool i0 = frustum.cubeInFrustum(c0.aabb);
            bool i1 = frustum.cubeInFrustum(c1.aabb);
            if (i0 && !i1)
                return -1;
            if (i1 && !i0)
                return 1;
            int t0 = (int)((now - c0.dirtiedTime) / 2000L);
            int t1 = (int)((now - c1.dirtiedTime) / 2000L);
            if (t0 < t1)
                return -1;
            if (t0 > t1)
                return 1;
            return c0.distanceToSqr(player) < c1.distanceToSqr(player) ? -1 : 1;
        }
    }
}
