using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IEntityViewsDB
    {
        FasterReadOnlyList<T> QueryEntityViews<T>() where T : EntityView;
        FasterReadOnlyList<T> QueryGroupedEntityViews<T>(int group) where T : EntityView;

        T[] QueryEntityViewsAsArray<T>(out int    count) where T : IEntityView;
        T[] QueryGroupedEntityViewsAsArray<T>(int group, out int count) where T : EntityView;

        bool TryQueryEntityView<T>(EGID ID, out T entityView) where T : EntityView;
        T    QueryEntityView<T>(EGID entityGID) where T : EntityView;
    }
}