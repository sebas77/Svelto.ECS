using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.Common;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    static class BurstCompatibleCounter
    {
        public static int counter;        
    }

    public class ComponentTypeID<T> where T : struct, _IInternalEntityComponent
    {
        static readonly SharedStaticWrapper<ComponentID, ComponentTypeID<T>> _id;

        public static ComponentID id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _id.Data;
        }

        /// <summary>
        /// c# Static constructors are guaranteed to be thread safe
        /// The runtime guarantees that a static constructor is only called once. So even if a type is called by multiple threads at the same time,
        /// the static constructor is always executed one time. To get a better understanding how this works, it helps to know what purpose it serves.
        /// </summary>
        static ComponentTypeID()
        {
            Init();
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard] 
        //SharedStatic values must be initialized from not burstified code
#endif
        internal static void Init()
        {
            if (_id.Data != 0)
                return;
            _id.Data = Interlocked.Increment(ref BurstCompatibleCounter.counter);
            ComponentTypeMap.Add(typeof(T), id);
        }
    }
}