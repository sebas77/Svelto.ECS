using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IEntityViewsDB
    {
        ReadOnlyCollectionStruct<T> QueryEntityViews<T>() where T : IEntityData;
        ReadOnlyCollectionStruct<T> QueryEntityViews<T>(int group) where T : IEntityData;

        T[] QueryEntityViewsCacheFriendly<T>(out int    count) where T : IEntityData;
        T[] QueryEntityViewsCacheFriendly<T>(int group, out int count) where T : IEntityData;

        bool TryQueryEntityView<T>(EGID ID, out T entityView) where T : IEntityData;
        T    QueryEntityView<T>(EGID entityGID) where T : class, IEntityData;
    }
}