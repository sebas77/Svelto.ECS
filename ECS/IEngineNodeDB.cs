using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IEngineEntityViewDB
    {
        FasterReadOnlyList<T> QueryEntityViews<T>() where T:EntityView<T>, new();
        FasterReadOnlyList<T> QueryMetaEntityViews<T>() where T: EntityView<T>, new();
        FasterReadOnlyList<T> QueryGroupedEntityViews<T>(int group) where T: EntityView<T>, new();
        
        T[] QueryEntityViewsAsArray<T>(out int count) where T: IEntityView;
        T[] QueryGroupedEntityViewsAsArray<T>(int @group, out int count) where T: IEntityView;
        
        ReadOnlyDictionary<int, T> QueryIndexableEntityViews<T>()  where T: IEntityView;
        bool TryQueryEntityView<T>(int ID, out T entityView) where T : IEntityView;
        T QueryEntityView<T>(int ID) where T: IEntityView;

        bool TryQueryMetaEntityView<T>(int metaEntityID, out T entityView)  where T: EntityView<T>, new();
        T QueryMetaEntityView<T>(int metaEntityID) where T: EntityView<T>, new();
    }
}

