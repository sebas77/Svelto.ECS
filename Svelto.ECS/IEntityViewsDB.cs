using System;
using Svelto.DataStructures;
using Svelto.Utilities;

namespace Svelto.ECS
{
    public interface IEntityViewsDB
    {
        //to use with EntityViews
        ReadOnlyCollectionStruct<T> QueryEntityViews<T>() where T : class, IEntityData;
        ReadOnlyCollectionStruct<T> QueryEntityViews<T>(int group) where T : class, IEntityData;

        //to use with EntityViews, EntityStructs and EntityViewStructs
        T[] QueryEntities<T>(out int    count) where T : IEntityData;
        T[] QueryEntities<T>(int group, out int count) where T : IEntityData;
        T[] QueryEntities<T>(EGID entityGID, out uint index) where T : IEntityData;

        //to use with EntityViews
        bool TryQueryEntityView<T>(EGID egid, out T entityView) where T : class, IEntityData;
        T    QueryEntityView<T>(EGID egid) where T : class, IEntityData;
        
        //to use with EntityViews, EntityStructs and EntityViewStructs
        void ExecuteOnEntity<T, W>(EGID egid, ref W value, ActionRef<T, W> action) where T : IEntityData;
        void ExecuteOnEntity<T>(EGID egid, ActionRef<T> action) where T : IEntityData;
        
        bool Exists<T>(EGID egid) where T : IEntityData;
        
        bool HasAny<T>() where T:IEntityData;
        bool HasAny<T>(int group) where T:IEntityData;
    }
}