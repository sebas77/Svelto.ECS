using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    class EntityViewsDB : IEntityViewsDB
    {
        internal EntityViewsDB(  Dictionary<Type, ITypeSafeList> entityViewsDB, 
                                Dictionary<Type, ITypeSafeDictionary> entityViewsDBdic,
                                Dictionary<Type, ITypeSafeList> metaEntityViewsDB,
                                Dictionary<int, Dictionary<Type, ITypeSafeList>>  groupEntityViewsDB)
        {
            _entityViewsDB = entityViewsDB;
            _entityViewsDBdic = entityViewsDBdic;
            _metaEntityViewsDB = metaEntityViewsDB;
            _groupEntityViewsDB = groupEntityViewsDB;
        }

        public FasterReadOnlyList<T> QueryEntityViews<T>() where T:EntityView, new()
        {
            var type = typeof(T);

            ITypeSafeList entityViews;

            if (_entityViewsDB.TryGetValue(type, out entityViews) == false)
                return RetrieveEmptyEntityViewList<T>();

            return new FasterReadOnlyList<T>((FasterList<T>)entityViews);
        }

        public FasterReadOnlyList<T> QueryGroupedEntityViews<T>(int @group) where T:EntityView, new()
        {
            Dictionary<Type, ITypeSafeList> entityViews;

            if (_groupEntityViewsDB.TryGetValue(group, out entityViews) == false)
                return RetrieveEmptyEntityViewList<T>();
            
            return new FasterReadOnlyList<T>(entityViews as FasterList<T>);
        }

        public T[] QueryEntityViewsAsArray<T>(out int count) where T : IEntityView
        {
            var type = typeof(T);
            count = 0;
            
            ITypeSafeList entityViews;

            if (_entityViewsDB.TryGetValue(type, out entityViews) == false)
                return RetrieveEmptyEntityViewArray<T>();
            
            var castedEntityViews = (FasterList<T>)entityViews;

            return FasterList < T > .NoVirt.ToArrayFast(castedEntityViews, out count);
        }
        
        public T[] QueryGroupedEntityViewsAsArray<T>(int @group, out int count) where T : IEntityView
        {
            var type = typeof(T);
            count = 0;
            
            Dictionary<Type, ITypeSafeList> entityViews;
            
            if (_groupEntityViewsDB.TryGetValue(group, out entityViews) == false)
                return RetrieveEmptyEntityViewArray<T>();
                       
            var castedEntityViews = (FasterList<T>)entityViews[type];

            count = castedEntityViews.Count;

            return FasterList<T>.NoVirt.ToArrayFast(castedEntityViews, out count);
        }

        public ReadOnlyDictionary<int, T> QueryIndexableEntityViews<T>() where T:IEntityView
        {
            var type = typeof(T);

            ITypeSafeDictionary entityViews;

            if (_entityViewsDBdic.TryGetValue(type, out entityViews) == false)
                return TypeSafeDictionary<T>.Default;

            return new ReadOnlyDictionary<int, T>(entityViews as Dictionary<int, T>);
        }

        public bool TryQueryEntityView<T>(int ID, out T entityView) where T : IEntityView
        {
            var type = typeof(T);

            T internalEntityView;

            ITypeSafeDictionary entityViews;
            TypeSafeDictionary<T> casted;

            _entityViewsDBdic.TryGetValue(type, out entityViews);
            casted = entityViews as TypeSafeDictionary<T>;

            if (casted != null &&
                casted.TryGetValue(ID, out internalEntityView))
            {
                entityView = internalEntityView;

                return true;
            }

            entityView = default(T);

            return false;
        }

        public T QueryEntityView<T>(int ID) where T : IEntityView
        {
            var type = typeof(T);

            T internalEntityView; ITypeSafeDictionary entityViews;
            TypeSafeDictionary<T> casted;

            _entityViewsDBdic.TryGetValue(type, out entityViews);
            casted = entityViews as TypeSafeDictionary<T>;

            if (casted != null &&
                casted.TryGetValue(ID, out internalEntityView))
                return (T)internalEntityView;

            throw new Exception("EntityView Not Found");
        }

        public T QueryMetaEntityView<T>(int metaEntityID) where T:EntityView, new()
        {
            return QueryEntityView<T>(metaEntityID);
        }

        public bool TryQueryMetaEntityView<T>(int metaEntityID, out T entityView) where T:EntityView, new()
        {
            return TryQueryEntityView(metaEntityID, out entityView);
        }

        public FasterReadOnlyList<T> QueryMetaEntityViews<T>() where T:EntityView, new()
        {
            var type = typeof(T);

            ITypeSafeList entityViews;

            if (_metaEntityViewsDB.TryGetValue(type, out entityViews) == false)
                return RetrieveEmptyEntityViewList<T>();

            return new FasterReadOnlyList<T>((FasterList<T>)entityViews);
        }

        static FasterReadOnlyList<T> RetrieveEmptyEntityViewList<T>()
        {
            return FasterReadOnlyList<T>.DefaultList;
        }

        static T[] RetrieveEmptyEntityViewArray<T>()
        {
            return FasterList<T>.DefaultList.ToArrayFast();
        }

        readonly Dictionary<Type, ITypeSafeList>                  _entityViewsDB;
        readonly Dictionary<Type, ITypeSafeDictionary>            _entityViewsDBdic;
        readonly Dictionary<Type, ITypeSafeList>                  _metaEntityViewsDB;
        readonly Dictionary<int, Dictionary<Type, ITypeSafeList>> _groupEntityViewsDB;
    }
}
