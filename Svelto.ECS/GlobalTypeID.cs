#if UNITY_ECS
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.DataStructures;
using Unity.Burst;

namespace Svelto.ECS
{
    public class GlobalTypeID
    {
        internal static uint NextID<T>()
        {
            return (uint) (Interlocked.Increment(ref value) - 1);
        }

        static GlobalTypeID()
        {
            value = 0;
        }

        static int value;
    }
    
    static class EntityComponentID<T>
    {
        internal static readonly SharedStatic<uint> ID = SharedStatic<uint>.GetOrCreate<GlobalTypeID, T>();
    }
    
    interface IFiller 
    {
        void FillFromByteArray(EntityComponentInitializer init, NativeBag buffer);
    }

    class Filler<T>: IFiller where T : struct, IEntityComponent
    {
        void IFiller.FillFromByteArray(EntityComponentInitializer init, NativeBag buffer)
        {
            var component = buffer.Dequeue<T>();

            init.Init(component);
        }
    }

    static class EntityComponentIDMap
    {
        static readonly FasterList<IFiller> TYPE_IDS = new FasterList<IFiller>();

        internal static void Register<T>(IFiller entityBuilder) where T : struct, IEntityComponent
        {
            var location = EntityComponentID<T>.ID.Data = GlobalTypeID.NextID<T>();
            TYPE_IDS.Add(location, entityBuilder);
        }
        
        internal static IFiller GetTypeFromID(uint typeId)
        {
            return TYPE_IDS[typeId];
        }
    }
}
#endif