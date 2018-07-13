using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;
using Svelto.Utilities;

namespace Svelto.ECS.Internal
{
    class entitiesDB : IEntitiesDB
    {
        internal entitiesDB(Dictionary<int, Dictionary<Type, ITypeSafeDictionary>> groupEntityViewsDB,
            Dictionary<Type, FasterDictionary<int, ITypeSafeDictionary>> groupedGroups)
        {
            _groupEntityViewsDB = groupEntityViewsDB;
            _groupedGroups = groupedGroups;
        }

        public ReadOnlyCollectionStruct<T> QueryEntityViews<T>() where T:class, IEntityStruct
        {
            return QueryEntityViews<T>(ExclusiveGroup.StandardEntitiesGroup);
        }

        public ReadOnlyCollectionStruct<T> QueryEntityViews<T>(int @group) where T:class, IEntityStruct
        {
            Dictionary<Type, ITypeSafeDictionary> entitiesInGroupPerType;

            if (_groupEntityViewsDB.TryGetValue(group, out entitiesInGroupPerType) == false)
                return RetrieveEmptyEntityViewList<T>();

            ITypeSafeDictionary outList;
            if (entitiesInGroupPerType.TryGetValue(typeof(T), out outList) == false)
                return RetrieveEmptyEntityViewList<T>();

            return (outList as TypeSafeDictionary<T>).FasterValues;
        }

        public T[] QueryEntities<T>(out int count) where T : IEntityStruct
        {
            return QueryEntities<T>(ExclusiveGroup.StandardEntitiesGroup, out count);
        }
        
        public T[] QueryEntities<T>(int @group, out int count) where T : IEntityStruct
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

        public T[] QueryEntitiesAndIndex<T>(EGID entityGID, out uint index) where T : IEntityStruct
        {
            TypeSafeDictionary<T> casted;
            if (!FindSafeDictionary(entityGID, out casted))
            {
                index = 0;
                return null;
            }

            if (casted == null || casted.TryFindElementIndex(entityGID.entityID, out index) == false)
            {
                index = 0;
                return null;
            }

            int count;
            
            return QueryEntities<T>(entityGID.groupID, out count);
        }

        public bool TryQueryEntitiesAndIndex<T>(EGID entityGid, out uint index, out T[] array) where T : IEntityStruct
        {
            if ((array = QueryEntitiesAndIndex<T>(entityGid, out index)) != null)
                return true;
            
            return false;
        }

        public T QueryEntityView<T>(EGID entityGID) where T : class, IEntityStruct
        {
            T entityView;

            if (TryQueryEntityViewInGroup(entityGID, out entityView) == false)
                throw new Exception("Entity not found id: ".FastConcat(entityGID.entityID).FastConcat(" groupID: ").FastConcat(entityGID.groupID));

            return entityView;
        }

        public void ExecuteOnEntity<T, W>(EGID entityGID, ref W value, ActionRef<T, W> action) where T : IEntityStruct
        {
            TypeSafeDictionary<T> casted;
            if (FindSafeDictionary(entityGID, out casted))
            {
                if (casted != null)
                    if (casted.ExecuteOnEntityView(entityGID.entityID, ref value, action) == true)
                        return;
            }

            throw new Exception("Entity not found id: ".FastConcat(entityGID.entityID).FastConcat(" groupID: ").FastConcat(entityGID.groupID));
        }
        
        public void ExecuteOnEntity<T>(EGID entityGID, ActionRef<T> action) where T : IEntityStruct
        {
            TypeSafeDictionary<T> casted;
            if (FindSafeDictionary(entityGID, out casted))
            {
                if (casted != null)
                    if (casted.ExecuteOnEntityView(entityGID.entityID, action) == true)
                        return;
            }

            throw new Exception("Entity not found id: ".FastConcat(entityGID.entityID).FastConcat(" groupID: ").FastConcat(entityGID.groupID));
        }

        public void ExecuteOnEntity<T>(int id, ActionRef<T> action) where T : IEntityStruct
        {
            ExecuteOnEntity(new EGID(id, ExclusiveGroup.StandardEntitiesGroup), action);
        }

        public void ExecuteOnEntity<T>(int id, int groupid, ActionRef<T> action) where T : IEntityStruct
        {
            ExecuteOnEntity(new EGID(id, groupid), action);
        }

        public void ExecuteOnEntity<T, W>(int id, ref W value, ActionRef<T, W> action) where T : IEntityStruct
        {
            ExecuteOnEntity(new EGID(id, ExclusiveGroup.StandardEntitiesGroup), ref value, action);
        }

        public void ExecuteOnEntity<T, W>(int id, int groupid, ref W value, ActionRef<T, W> action) where T : IEntityStruct
        {
            ExecuteOnEntity(new EGID(id, groupid), ref value, action);
        }

        public void ExecuteOnEntities<T>(int groupID, ActionRef<T> action) where T : IEntityStruct
        {
            int count;
            var entities = QueryEntities<T>(groupID, out count);

            for (int i = 0; i < count; i++)
                action(ref entities[i]);
        }

        public void ExecuteOnEntities<T>(ActionRef<T> action) where T : IEntityStruct
        {
            int count;
            var entities = QueryEntities<T>(out count);

            for (int i = 0; i < count; i++)
                action(ref entities[i]);
        }

        public void ExecuteOnEntities<T, W>(int groupID, ref W value, ActionRef<T, W> action) where T : IEntityStruct
        {
            int count;
            var entities = QueryEntities<T>(groupID, out count);

            for (int i = 0; i < count; i++)
                action(ref entities[i], ref value);
        }

        public void ExecuteOnEntities<T, W>(ref W value, ActionRef<T, W> action) where T : IEntityStruct
        {
            int count;
            var entities = QueryEntities<T>(out count);

            for (int i = 0; i < count; i++)
                action(ref entities[i], ref value);
        }

        public void ExecuteOnAllEntities<T>(ActionRef<T> action) where T : IEntityStruct
        {
            int count;
            var typeSafeDictionaries = _groupedGroups[typeof(T)].GetFasterValuesBuffer(out count);
            for (int j = 0; j < count; j++)
            {
                int count2;
                var safedic = typeSafeDictionaries[j];
                TypeSafeDictionary<T> casted = safedic as TypeSafeDictionary<T>;
                var entities = casted.GetFasterValuesBuffer(out count2);
                for (int i = 0; i < count2; i++)
                    action(ref entities[i]);
            }
        }

        public void ExecuteOnAllEntities<T, W>(ref W value, ActionRef<T, W> action) where T : IEntityStruct
        {
            int count;
            var typeSafeDictionaries = _groupedGroups[typeof(T)].GetFasterValuesBuffer(out count);
            for (int j = 0; j < count; j++)
            {
                int count2;
                var safedic = typeSafeDictionaries[j];
                TypeSafeDictionary<T> casted = safedic as TypeSafeDictionary<T>;
                var entities = casted.GetFasterValuesBuffer(out count2);
                for (int i = 0; i < count2; i++)
                    action(ref entities[i], ref value);
            }
        }

        public bool Exists<T>(EGID entityGID) where T : IEntityStruct
        {
            TypeSafeDictionary<T> casted;
            if (!FindSafeDictionary(entityGID, out casted)) return false;

            if (casted != null &&
                casted.ContainsKey(entityGID.entityID))
            {
                return true;
            }

            return false;
        }

        bool FindSafeDictionary<T>(EGID entityGID, out TypeSafeDictionary<T> casted) where T : IEntityStruct
        {
            var type = typeof(T);

            ITypeSafeDictionary entityViews;

            Dictionary<Type, ITypeSafeDictionary> entitiesInGroupPerType;
            if (_groupEntityViewsDB.TryGetValue(entityGID.groupID, out entitiesInGroupPerType) == false)
            {
                casted = null;
                return false;
            }

            entitiesInGroupPerType.TryGetValue(type, out entityViews);
            casted = entityViews as TypeSafeDictionary<T>;
            return true;
        }

        public bool HasAny<T>() where T : IEntityStruct
        {
            int count;
            QueryEntities<T>(out count);
            return count > 0;
        }

        public bool HasAny<T>(int @group) where T : IEntityStruct
        {
            int count;
            QueryEntities<T>(group, out count);
            return count > 0;
        }

        public bool TryQueryEntityView<T>(EGID entityegid, out T entityView) where T : class, IEntityStruct
        {
            return TryQueryEntityViewInGroup(entityegid, out entityView);
        }

        bool TryQueryEntityViewInGroup<T>(EGID entityGID, out T entityView) where T:IEntityStruct
        {
            TypeSafeDictionary<T> casted;
            if (!FindSafeDictionary(entityGID, out casted))
            {
                entityView = default(T);
                return false;
            }

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
        Dictionary<Type, FasterDictionary<int, ITypeSafeDictionary>> _groupedGroups;
    }
}
