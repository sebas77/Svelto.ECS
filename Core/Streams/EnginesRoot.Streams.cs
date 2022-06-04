using System.Runtime.CompilerServices;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Consumer<T> GenerateConsumer<T>(string name, uint capacity) where T : unmanaged, IBaseEntityComponent
        {
            return _entityStreams.GenerateConsumer<T>(name, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Consumer<T> GenerateConsumer<T>(ExclusiveGroupStruct group, string name, uint capacity)
            where T : unmanaged, IBaseEntityComponent
        {
            return _entityStreams.GenerateConsumer<T>(@group, name, capacity);
        }

        internal readonly EntitiesStreams _entityStreams;
    }
}