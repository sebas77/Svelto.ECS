using System;
using Svelto.DataStructures;
using Svelto.Utilities;

namespace Svelto.ECS
{
    public interface IEntityViewsDB
    {
        //to use with EntityViews
        ReadOnlyCollectionStruct<T> QueryEntityViews<T>() where T : class, IEntityStruct;
        ReadOnlyCollectionStruct<T> QueryEntityViews<T>(int group) where T : class, IEntityStruct;

        //to use with EntityViews, EntityStructs and EntityViewStructs
        T[] QueryEntities<T>(out int    count) where T : IEntityStruct;
        T[] QueryEntities<T>(int group, out int count) where T : IEntityStruct;
        T[] QueryEntities<T>(EGID entityGID, out uint index) where T : IEntityStruct;

        //to use with EntityViews
        bool TryQueryEntityView<T>(EGID egid, out T entityView) where T : class, IEntityStruct;
        T    QueryEntityView<T>(EGID egid) where T : class, IEntityStruct;
        
        //to use with EntityViews, EntityStructs and EntityViewStructs
        void ExecuteOnEntity<T, W>(EGID egid, ref W value, ActionRef<T, W> action) where T : IEntityStruct;
        void ExecuteOnEntity<T>(EGID egid, ActionRef<T> action) where T : IEntityStruct;
        
        bool Exists<T>(EGID egid) where T : IEntityStruct;
        
        bool HasAny<T>() where T:IEntityStruct;
        bool HasAny<T>(int group) where T:IEntityStruct;
    }
}