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
        bool TryQueryEntityView<T>(EGID ID, out T entityView) where T : class, IEntityData;
        T    QueryEntityView<T>(EGID entityGID) where T : class, IEntityData;
        
        bool EntityExists<T>(EGID ID) where T : IEntityData;
    }
}