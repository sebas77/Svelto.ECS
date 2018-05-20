using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IEntityViewsDB
    {
        //to use with EntityViews, EntityStructs and EntityViewStructs
        ReadOnlyCollectionStruct<T> QueryEntities<T>() where T : IEntityData;
        ReadOnlyCollectionStruct<T> QueryEntities<T>(int group) where T : IEntityData;

        //to use with EntityStructs and EntityViewStructs
        T[] QueryEntitiesCacheFriendly<T>(out int    count) where T : struct, IEntityData;
        T[] QueryEntitiesCacheFriendly<T>(int group, out int count) where T : struct, IEntityData;

        //to use with EntityViews
        bool TryQueryEntityView<T>(EGID egid, out T entityView) where T : IEntityData;
        T    QueryEntityView<T>(EGID egid) where T : class, IEntityData;
        
        bool Exists<T>(EGID egid) where T : IEntityData;
        void Fetch<T>(out T entity) where T:IEntityData;
        bool Has<T>() where T:IEntityData;
    }
}