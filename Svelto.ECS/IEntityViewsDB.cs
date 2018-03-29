using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IEntityViewsDB
    {
        FasterReadOnlyList<T> QueryEntityViews<T>() where T : EntityView;
        FasterReadOnlyList<T> QueryGroupedEntityViews<T>(int group) where T : EntityView;

        T[] QueryEntityViewsAsArray<T>(out int    count) where T : IEntityView;
        T[] QueryGroupedEntityViewsAsArray<T>(int group, out int count) where T : IEntityView;

        bool TryQueryEntityView<T>(int ID, out T entityView) where T : EntityView;
        T    QueryEntityView<T>(int    ID) where T : EntityView;
        
        bool TryQueryEntityViewInGroup<T>(int entityID, int groupID, out T entityView) where T : EntityView;
        T    QueryEntityViewInGroup<T>(int entityID, int groupID) where T : EntityView;
    }
}