using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    class EntityViewsDB : IEntityViewsDB
    {
        internal EntityViewsDB(  Dictionary<Type, ITypeSafeList> entityViewsDB,
                                Dictionary<int, Dictionary<Type, ITypeSafeDictionary>> entityViewsDBdic,
                                Dictionary<int, Dictionary<Type, ITypeSafeList>>  groupEntityViewsDB)
        {
            _entityViewsDB = entityViewsDB;
            _groupedEntityViewsDBDic = entityViewsDBdic;
            _groupEntityViewsDB = groupEntityViewsDB;
        }

        public FasterReadOnlyList<T> QueryEntityViews<T>() where T:EntityView
        {
            var type = typeof(T);

            ITypeSafeList entityViews;

            if (_entityViewsDB.TryGetValue(type, out entityViews) == false)
                return RetrieveEmptyEntityViewList<T>();

            return new FasterReadOnlyList<T>((FasterList<T>)entityViews);
        }

        public FasterReadOnlyList<T> QueryGroupedEntityViews<T>(int @group) where T:EntityView
        {
            Dictionary<Type, ITypeSafeList> entitiesInGroupPerType;

            if (_groupEntityViewsDB.TryGetValue(group, out entitiesInGroupPerType) == false)
                return RetrieveEmptyEntityViewList<T>();

            ITypeSafeList outList;
            if (entitiesInGroupPerType.TryGetValue(typeof(T), out outList) == false)
                return RetrieveEmptyEntityViewList<T>();
            
            return new FasterReadOnlyList<T>((FasterList<T>) outList);
        }

        public T[] QueryEntityViewsAsArray<T>(out int count) where T : IEntityView
        {
            var type = typeof(T);
            count = 0;
            
            ITypeSafeList entityViews;

            if (_entityViewsDB.TryGetValue(type, out entityViews) == false)
                return RetrieveEmptyEntityViewArray<T>();
            
            return FasterList<T>.NoVirt.ToArrayFast((FasterList<T>)entityViews, out count);
        }
        
        public T[] QueryGroupedEntityViewsAsArray<T>(int @group, out int count) where T : IEntityView
        {
            var type = typeof(T);
            count = 0;
            
            Dictionary<Type, ITypeSafeList> entitiesInGroupPerType;
            
            if (_groupEntityViewsDB.TryGetValue(group, out entitiesInGroupPerType) == false)
                return RetrieveEmptyEntityViewArray<T>();
            
            ITypeSafeList outList;
            if (entitiesInGroupPerType.TryGetValue(typeof(T), out outList) == false)
                return RetrieveEmptyEntityViewArray<T>();
                       
            return FasterList<T>.NoVirt.ToArrayFast((FasterList<T>)entitiesInGroupPerType[type], out count);
        }

        public T QueryEntityView<T>(int entityID) where T:EntityView
        {
            return QueryEntityViewInGroup<T>(entityID, ExclusiveGroups.StandardEntity);
        }

        public bool TryQueryEntityView<T>(int entityID, out T entityView) where T:EntityView
        {
            return TryQueryEntityViewInGroup(entityID, ExclusiveGroups.StandardEntity, out entityView);
        }
        
        public T QueryEntityViewInGroup<T>(int entityID, int groupID) where T:EntityView
        {
            T entityView;
            
            TryQueryEntityView(entityID, groupID, _groupedEntityViewsDBDic, out entityView);

            return entityView;
        }

        public bool TryQueryEntityViewInGroup<T>(int entityID, int groupID, out T entityView) where T:EntityView
        {
            return TryQueryEntityView(entityID, groupID, _groupedEntityViewsDBDic, out entityView);
        }

        static FasterReadOnlyList<T> RetrieveEmptyEntityViewList<T>()
        {
            return FasterReadOnlyList<T>.DefaultList;
        }

        static T[] RetrieveEmptyEntityViewArray<T>()
        {
            return FasterList<T>.DefaultList.ToArrayFast();
        }
        
        static bool TryQueryEntityView<T>(int ID, int groupID, Dictionary<int, Dictionary<Type, ITypeSafeDictionary>> entityDic, out T entityView) where T : EntityView
        {
            var type = typeof(T);

            T internalEntityView;

            ITypeSafeDictionary   entityViews;
            TypeSafeDictionary<T> casted;

            Dictionary<Type, ITypeSafeDictionary> @group;
            if (entityDic.TryGetValue(groupID, out group) == false)
                throw new Exception("Group not found");

            group.TryGetValue(type, out entityViews);
            casted = entityViews as TypeSafeDictionary<T>;

            if (casted != null &&
                casted.TryGetValue(ID, out internalEntityView))
            {
                entityView = internalEntityView;

                return true;
            }

            entityView = null;

            return false;
        }

        readonly Dictionary<int, Dictionary<Type, ITypeSafeList>>       _groupEntityViewsDB;
        readonly Dictionary<int, Dictionary<Type, ITypeSafeDictionary>> _groupedEntityViewsDBDic;

        readonly Dictionary<Type, ITypeSafeList> _entityViewsDB;
    }
}
