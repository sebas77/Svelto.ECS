using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    class EntityViewsDB : IEntityViewsDB
    {
        internal EntityViewsDB(Dictionary<int, Dictionary<Type, ITypeSafeDictionary>>  groupEntityViewsDB)
        {
            _groupEntityViewsDB = groupEntityViewsDB;
        }

        public ReadOnlyCollectionStruct<T> QueryEntities<T>() where T:IEntityData
        {
            return QueryEntities<T>(ExclusiveGroups.StandardEntity);
        }

        public ReadOnlyCollectionStruct<T> QueryEntities<T>(int @group) where T:IEntityData
        {
            Dictionary<Type, ITypeSafeDictionary> entitiesInGroupPerType;

            if (_groupEntityViewsDB.TryGetValue(group, out entitiesInGroupPerType) == false)
                return RetrieveEmptyEntityViewList<T>();

            ITypeSafeDictionary outList;
            if (entitiesInGroupPerType.TryGetValue(typeof(T), out outList) == false)
                return RetrieveEmptyEntityViewList<T>();

            return (outList as TypeSafeDictionary<T>).FasterValues;
        }

        public T[] QueryEntitiesCacheFriendly<T>(out int count) where T : struct, IEntityData
        {
            return QueryEntitiesCacheFriendly<T>(ExclusiveGroups.StandardEntity, out count);
        }
        
        public T[] QueryEntitiesCacheFriendly<T>(int @group, out int count) where T : struct, IEntityData
        {
            count = 0;
            
            Dictionary<Type, ITypeSafeDictionary> entitiesInGroupPerType;
            
            if (_groupEntityViewsDB.TryGetValue(group, out entitiesInGroupPerType) == false)
                return RetrieveEmptyEntityViewArray<T>();
            
            ITypeSafeDictionary typeSafeDictionary;
            if (entitiesInGroupPerType.TryGetValue(typeof(T), out typeSafeDictionary) == false)
                return RetrieveEmptyEntityViewArray<T>();

            return ((TypeSafeDictionary<T>)typeSafeDictionary).GetFasterValuesBuffer(out count);
        }

        public T QueryEntityView<T>(EGID entityGID) where T : class, IEntityData
        {
            T entityView;

            TryQueryEntityViewInGroup(entityGID, out entityView);

            return entityView;
        }

        public bool EntityExists<T>(EGID entityGID) where T : IEntityData
        {
            var type = typeof(T);

            ITypeSafeDictionary entityViews;
            
            Dictionary<Type, ITypeSafeDictionary> entitiesInGroupPerType;
            if (_groupEntityViewsDB.TryGetValue(entityGID.groupID, out entitiesInGroupPerType) == false)
            {
                return false;
            }

            entitiesInGroupPerType.TryGetValue(type, out entityViews);
            var casted = entityViews as TypeSafeDictionary<T>;

            if (casted != null &&
                casted.ContainsKey(entityGID.entityID))
            {
                return true;
            }

            return false;
        }
        
        public bool TryQueryEntityView<T>(EGID entityegid, out T entityView) where T : class, IEntityData
        {
            return TryQueryEntityViewInGroup(entityegid, out entityView);
        }

        bool TryQueryEntityViewInGroup<T>(EGID entityGID, out T entityView) where T:class, IEntityData
        {
            var type = typeof(T);

            ITypeSafeDictionary entityViews;
            
            Dictionary<Type, ITypeSafeDictionary> entitiesInGroupPerType;
            if (_groupEntityViewsDB.TryGetValue(entityGID.groupID, out entitiesInGroupPerType) == false)
            {
                entityView = default(T);
                return false;
            }

            entitiesInGroupPerType.TryGetValue(type, out entityViews);
            var casted = entityViews as TypeSafeDictionary<T>;

            if (casted != null &&
                casted.TryGetValue(entityGID.entityID, out entityView))
            {
                return true;
            }

            entityView = default(T);

            return false;
        }

        static ReadOnlyCollectionStruct<T> RetrieveEmptyEntityViewList<T>()
        {
            return ReadOnlyCollectionStruct<T>.DefaultList;
        }

        static T[] RetrieveEmptyEntityViewArray<T>()
        {
            return FasterList<T>.DefaultList.ToArrayFast();
        }
     
        //grouped set of entity views, this is the standard way to handle entity views
        readonly Dictionary<int, Dictionary<Type, ITypeSafeDictionary>> _groupEntityViewsDB;
    }
}
