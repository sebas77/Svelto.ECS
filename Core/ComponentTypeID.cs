using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.Common;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public static class BurstCompatibleCounter
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

        static ComponentTypeID()
        {
            Init();
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard] 
        //SharedStatic values must be initialized from not burstified code
#endif
        static void Init()
        {
            _id.Data = Interlocked.Increment(ref BurstCompatibleCounter.counter);
            ComponentTypeMap.Add(typeof(T), id);
        }
    }
}