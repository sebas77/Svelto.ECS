using System.Threading;

namespace Svelto.Utilities
{
    public static class ThreadUtility
    {
        public static void MemoryBarrier()
        {
#if NETFX_CORE || NET_4_6
        Interlocked.MemoryBarrier();
#else
            Thread.MemoryBarrier();
#endif
        }
    }
}