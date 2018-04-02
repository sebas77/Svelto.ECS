using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    class EntityViewsDB : IEntityViewsDB
    {
        internal EntityViewsDB(  Dictionary<Type, ITypeSafeList> entityViewsDB,
                                Dictionary<Type, ITypeSafeDictionary> entityViewsDBdic,
                                Dictionary<int, Dictionary<Type, ITypeSafeList>>  groupEntityViewsDB)
        {
            _globalEntityViewsDB = entityViewsDB;
            _groupedEntityViewsDBDic = entityViewsDBdic;
            _groupEntityViewsDB = groupEntityViewsDB;
        }

        public FasterReadOnlyList<T> QueryEntityViews<T>() where T:EntityView
        {
            var type = typeof(T);

            ITypeSafeList entityViews;

            if (_globalEntityViewsDB.TryGetValue(type, out entityViews) == false)
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

            if (_globalEntityViewsDB.TryGetValue(type, out entityViews) == false)
                return RetrieveEmptyEntityViewArray<T>();
            
            return FasterList<T>.NoVirt.ToArrayFast((FasterList<T>)entityViews, out count);
        }
        
        public T[] QueryGroupedEntityViewsAsArray<T>(int @group, out int count) where T : EntityView
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

        public T QueryEntityView<T>(EGID entityGID) where T : EntityView
        {
            T entityView;

            TryQueryEntityViewInGroup(entityGID, out entityView);

            return entityView;
        }

        public bool TryQueryEntityView<T>(EGID entityegid, out T entityView) where T : EntityView
        {
            return TryQueryEntityViewInGroup(entityegid, out entityView);
        }

        bool TryQueryEntityViewInGroup<T>(EGID entityGID, out T entityView) where T:EntityView
        {
            var type = typeof(T);

            T internalEntityView;

            ITypeSafeDictionary entityViews;
            TypeSafeDictionaryForClass<T> casted;

            _groupedEntityViewsDBDic.TryGetValue(type, out entityViews);
            casted = entityViews as TypeSafeDictionaryForClass<T>;

            if (casted != null &&
                casted.TryGetValue(entityGID.GID, out internalEntityView))
            {
                entityView = internalEntityView;

                return true;
            }

            entityView = null;

            return false;
        }

        static FasterReadOnlyList<T> RetrieveEmptyEntityViewList<T>()
        {
            return FasterReadOnlyList<T>.DefaultList;
        }

        static T[] RetrieveEmptyEntityViewArray<T>()
        {
            return FasterList<T>.DefaultList.ToArrayFast();
        }
        
     
        //grouped set of entity views, this is the standard way to handle entity views
        readonly Dictionary<int, Dictionary<Type, ITypeSafeList>> _groupEntityViewsDB;
        //Global pool of entity views when engines want to manage entityViews regardless
        //the group
        readonly Dictionary<Type, ITypeSafeList> _globalEntityViewsDB;
        //indexable entity views when the entity ID is known. Usually useful to handle
        //event based logic.
        readonly Dictionary<Type, ITypeSafeDictionary> _groupedEntityViewsDBDic;
    }
}
