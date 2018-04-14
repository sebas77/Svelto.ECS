using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IEntityViewsDB
    {
        FasterReadOnlyList<T> QueryEntityViews<T>() where T : IEntityData;
        FasterReadOnlyList<T> QueryGroupedEntityViews<T>(int group) where T : IEntityData;

        T[] QueryEntityViewsAsArray<T>(out int    count) where T : IEntityData;
        T[] QueryGroupedEntityViewsAsArray<T>(int group, out int count) where T : IEntityData;

        bool TryQueryEntityView<T>(EGID ID, out T entityView) where T : IEntityData;
        T    QueryEntityView<T>(EGID entityGID) where T : IEntityData;
    }
}