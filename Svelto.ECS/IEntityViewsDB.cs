using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IEntityViewsDB
    {
        FasterReadOnlyList<T> QueryEntityViews<T>() where T : EntityView;
        FasterReadOnlyList<T> QueryMetaEntityViews<T>() where T : EntityView;
        FasterReadOnlyList<T> QueryGroupedEntityViews<T>(int group) where T : EntityView;

        T[] QueryEntityViewsAsArray<T>(out int    count) where T : IEntityView;
        T[] QueryGroupedEntityViewsAsArray<T>(int group, out int count) where T : IEntityView;

        ReadOnlyDictionary<int, T> QueryIndexableEntityViews<T>() where T : EntityView;
        ReadOnlyDictionary<int, T> QueryIndexableMetaEntityViews<T>() where T : EntityView;

        bool TryQueryEntityView<T>(int ID, out T entityView) where T : EntityView;
        T    QueryEntityView<T>(int    ID) where T : EntityView;

        bool TryQueryMetaEntityView<T>(int metaEntityID, out T entityView) where T : EntityView;
        T    QueryMetaEntityView<T>(int    metaEntityID) where T : EntityView;
    }
}