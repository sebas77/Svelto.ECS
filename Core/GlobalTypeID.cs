using System.Threading;
using Svelto.Common;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS
{
    public class GlobalTypeID
    {
        internal static uint NextID<T>() { return (uint) (Interlocked.Increment(ref value) - 1); }

        static GlobalTypeID() { value = 0; }

        static int value;
    }

    interface IFiller
    {
        void FillFromByteArray(EntityInitializer init, NativeBag buffer);
    }

    class Filler<T> : IFiller where T : struct, IBaseEntityComponent
    {
        static Filler()
        {
            DBC.ECS.Check.Require(TypeType.isUnmanaged<T>() == true, "invalid type used");
        }

        //it's an internal interface
        public void FillFromByteArray(EntityInitializer init, NativeBag buffer)
        {
            var component = buffer.Dequeue<T>();

            init.Init(component);
        }
    }

#if UNITY_NATIVE //at the moment I am still considering NativeOperations useful only for Unity
    static class EntityComponentID<T>
    {
        internal static readonly Unity.Burst.SharedStatic<uint> ID =
            Unity.Burst.SharedStatic<uint>.GetOrCreate<GlobalTypeID, T>();
    }

    static class EntityComponentIDMap
    {
        static readonly Svelto.DataStructures.FasterList<IFiller> TYPE_IDS;

        static EntityComponentIDMap()
        {
            TYPE_IDS = new Svelto.DataStructures.FasterList<IFiller>();
        }

        internal static void Register<T>(IFiller entityBuilder) where T : struct, IBaseEntityComponent
        {
            var location = EntityComponentID<T>.ID.Data = GlobalTypeID.NextID<T>();
            TYPE_IDS.AddAt(location, entityBuilder);
        }

        internal static IFiller GetTypeFromID(uint typeId) { return TYPE_IDS[typeId]; }
    }
#endif
}