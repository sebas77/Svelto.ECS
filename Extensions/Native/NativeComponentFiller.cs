using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    interface IFiller
    {
        void FillFromByteArray(EntityInitializer init, NativeBag buffer);
    }

    class Filler<T> : IFiller where T : struct, _IInternalEntityComponent
    {
        static Filler()
        {
            DBC.ECS.Check.Require(TypeCache<T>.isUnmanaged == true, "invalid type used");
        }

        //it's an internal interface
        public void FillFromByteArray(EntityInitializer init, NativeBag buffer)
        {
            var component = buffer.Dequeue<T>();

            init.Init(component);
        }
    }

#if UNITY_NATIVE //at the moment I am still considering NativeOperations useful only for Unity
    static class EntityComponentIDMap
    {
        static readonly Svelto.DataStructures.FasterList<IFiller> TYPE_IDS;

        static EntityComponentIDMap()
        {
            TYPE_IDS = new Svelto.DataStructures.FasterList<IFiller>();
        }

        internal static void Register<T>(IFiller entityBuilder) where T : struct, _IInternalEntityComponent
        {
            ComponentID location = ComponentTypeID<T>.id;
            TYPE_IDS.AddAt(location, entityBuilder);
        }

        internal static IFiller GetBuilderFromID(uint typeId) { return TYPE_IDS[typeId]; }
    }
#endif
}