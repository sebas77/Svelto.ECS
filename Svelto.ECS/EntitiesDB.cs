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
            TypeSafeDictionary<T> typeSafeDictionary;
            if (QueryEntitySafeDictionary(@group, out typeSafeDictionary) == false) return RetrieveEmptyEntityViewList<T>();

            return typeSafeDictionary.FasterValues;
        }

        public T[] QueryEntities<T>(out int count) where T : IEntityStruct
        {
            return QueryEntities<T>(ExclusiveGroup.StandardEntitiesGroup, out count);
        }
        
        public T[] QueryEntities<T>(int @group, out int count) where T : IEntityStruct
        {
            TypeSafeDictionary<T> typeSafeDictionary;
            count = 0;
            if (QueryEntitySafeDictionary(@group, out typeSafeDictionary) == false) return RetrieveEmptyEntityViewArray<T>();

            return typeSafeDictionary.GetFasterValuesBuffer(out count);
        }
        
        public EGIDMapper<T> QueryMappedEntities<T>() where T : IEntityStruct
        {
            return QueryMappedEntities<T>(ExclusiveGroup.StandardEntitiesGroup);
        }
        
        public EGIDMapper<T> QueryMappedEntities<T>(int groupID) where T : IEntityStruct
        {
            TypeSafeDictionary<T> typeSafeDictionary;
            
            if (QueryEntitySafeDictionary(groupID, out typeSafeDictionary) == false) 
                throw new EntitiesDBException("Entity group not found type: ".FastConcat(typeof(T)).FastConcat(" groupID: ").FastConcat(groupID));

            EGIDMapper<T> mapper;
            mapper.map = typeSafeDictionary;

            int count;
            typeSafeDictionary.GetFasterValuesBuffer(out count);

            return mapper;
        }

        public T[] QueryEntitiesAndIndex<T>(EGID entityGID, out uint index) where T : IEntityStruct
        {
            T[] array;
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGID, out index)) != null)
                return array;
            
            throw new EntitiesDBException("Entity not found id: ".FastConcat(entityGID.entityID).FastConcat(" groupID: ").FastConcat(entityGID.groupID)); 
        }
        
        public bool TryQueryEntitiesAndIndex<T>(EGID entityGid, out uint index, out T[] array) where T : IEntityStruct
        {
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGid, out index)) != null)
                return true;
            
            return false;
        }

        public T QueryEntityView<T>(EGID entityGID) where T : class, IEntityStruct
        {
            T entityView;

            if (TryQueryEntityViewInGroupInternal(entityGID, out entityView) == false)
                throw new EntitiesDBException("Entity not found id: ".FastConcat(entityGID.entityID).FastConcat(" groupID: ").FastConcat(entityGID.groupID));

            return entityView;
        }

        public void ExecuteOnEntity<T, W>(EGID entityGID, ref W value, ActionRef<T, W> action) where T : IEntityStruct
        {
            TypeSafeDictionary<T> casted;
            if (QueryEntitySafeDictionary(entityGID.groupID, out casted))
            {
                if (casted != null)
                    if (casted.ExecuteOnEntityView(entityGID.entityID, ref value, action) == true)
                        return;
            }

            throw new EntitiesDBException("Entity not found id: ".FastConcat(entityGID.entityID).FastConcat(" groupID: ").FastConcat(entityGID.groupID));
        }
        
        public void ExecuteOnEntity<T>(EGID entityGID, ActionRef<T> action) where T : IEntityStruct
        {
            TypeSafeDictionary<T> casted;
            if (QueryEntitySafeDictionary(entityGID.groupID, out casted))
            {
                if (casted != null)
                    if (casted.ExecuteOnEntityView(entityGID.entityID, action) == true)
                        return;
            }

            throw new EntitiesDBException("Entity not found id: ".FastConcat(entityGID.entityID).FastConcat(" groupID: ").FastConcat(entityGID.groupID));
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
            int                   count;
            TypeSafeDictionary<T> typeSafeDictionary;
            if (QueryEntitySafeDictionary(@groupID, out typeSafeDictionary) == false) return;
            
            var entities = typeSafeDictionary.GetFasterValuesBuffer(out count);
            
            for (var i = 0; i < count; i++)
                action(ref entities[i]);
            
            SafetyChecks(typeSafeDictionary, count);
        }

        static void SafetyChecks<T>(TypeSafeDictionary<T> typeSafeDictionary, int count) where T : IEntityStruct
        {
            if (typeSafeDictionary.Count != count)
                throw new EntitiesDBException("Entities cannot be swapped or removed during an iteration");
        }

        public void ExecuteOnEntities<T>(ActionRef<T> action) where T : IEntityStruct
        {
            ExecuteOnEntities(ExclusiveGroup.StandardEntitiesGroup, action);
        }

        public void ExecuteOnEntities<T, W>(int groupID, ref W value, ActionRef<T, W> action) where T : IEntityStruct
        {
            int                   count;
            TypeSafeDictionary<T> typeSafeDictionary;
            if (QueryEntitySafeDictionary(@groupID, out typeSafeDictionary) == false) return;
            
            var entities = typeSafeDictionary.GetFasterValuesBuffer(out count);
            
            for (var i = 0; i < count; i++)
                action(ref entities[i], ref value);
            
            SafetyChecks(typeSafeDictionary, count);
        }

        public void ExecuteOnEntities<T, W>(ref W value, ActionRef<T, W> action) where T : IEntityStruct
        {
            ExecuteOnEntities(ExclusiveGroup.StandardEntitiesGroup, ref value, action);
        }
        
        public void ExecuteOnEntities<T, T1, W>(W value, ActionRef<T, T1, W> action) where T : IEntityStruct where T1 : IEntityStruct
        {
            ExecuteOnEntities(ExclusiveGroup.StandardEntitiesGroup, ref value, action);
        }

        public void ExecuteOnAllEntities<T>(ActionRef<T> action) where T : IEntityStruct
        {
            var type = typeof(T);
            FasterDictionary<int, ITypeSafeDictionary> dic;
            if (_groupedGroups.TryGetValue(type, out dic))
            {
                int count;
                var typeSafeDictionaries = dic.GetFasterValuesBuffer(out count);
                for (int j = 0; j < count; j++)
                {
                    int innerCount;
                    var typeSafeDictionary = typeSafeDictionaries[j];
                    var casted             = typeSafeDictionary as TypeSafeDictionary<T>;
                    
                    var entities = casted.GetFasterValuesBuffer(out innerCount);

                    for (int i = 0; i < innerCount; i++)
                        action(ref entities[i]);
                    
                    SafetyChecks(casted, count);
                }
            }
        }

        public void ExecuteOnAllEntities<T, W>(ref W value, ActionRef<T, W> action) where T : IEntityStruct
        {
            var  type = typeof(T);
            FasterDictionary<int, ITypeSafeDictionary> dic;
            if (_groupedGroups.TryGetValue(type, out dic))
            {
                int count;
                var typeSafeDictionaries = dic.GetFasterValuesBuffer(out count);
                for (int j = 0; j < count; j++)
                {
                    int innerCount;
                    var typeSafeDictionary = typeSafeDictionaries[j];
                    var casted   = typeSafeDictionary as TypeSafeDictionary<T>;
                    
                    var entities = casted.GetFasterValuesBuffer(out innerCount);

                    for (int i = 0; i < innerCount; i++)
                        action(ref entities[i], ref value);
                    
                    SafetyChecks(casted, count);
                }
            }
        }

        public void ExecuteOnEntities<T, T1>(int group, ActionRef<T, T1> action) where T : IEntityStruct where T1 : IEntityStruct
        {
            int count;
            TypeSafeDictionary<T> typeSafeDictionary;
            if (QueryEntitySafeDictionary(@group, out typeSafeDictionary) == false) return;
            
            var entities  = typeSafeDictionary.GetFasterValuesBuffer(out count);

            EGIDMapper<T1> map = QueryMappedEntities<T1>(group);
            
            for (var i = 0; i < count; i++)
            {
                uint index;
                action(ref entities[i], ref map.entities(entities[i].ID, out index)[index]);
            }

            SafetyChecks(typeSafeDictionary, count);
        }
        
        public void ExecuteOnEntities<T, T1, W>(int group, ref W value, ActionRef<T, T1, W> action) where T : IEntityStruct where T1 : IEntityStruct
        {
            int                   count;
            TypeSafeDictionary<T> typeSafeDictionary;
            if (QueryEntitySafeDictionary(@group, out typeSafeDictionary) == false) return;
            
            var entities = typeSafeDictionary.GetFasterValuesBuffer(out count);

            EGIDMapper<T1> map = QueryMappedEntities<T1>(group);
            
            for (var i = 0; i < count; i++)
            {
                uint index;
                action(ref entities[i], ref map.entities(entities[i].ID, out index)[index], ref value);
            }

            SafetyChecks(typeSafeDictionary, count);
        }

        public void ExecuteOnEntities<T, T1>(ActionRef<T, T1> action) where T : IEntityStruct where T1 : IEntityStruct
        {
            ExecuteOnEntities(ExclusiveGroup.StandardEntitiesGroup, action);
        }
        
        public void ExecuteOnEntities<T, T1, W>(ref W value, ActionRef<T, T1, W> action) where T : IEntityStruct where T1 : IEntityStruct
        {
            ExecuteOnEntities(ExclusiveGroup.StandardEntitiesGroup, ref value, action);
        }

        public bool Exists<T>(EGID entityGID) where T : IEntityStruct
        {
            TypeSafeDictionary<T> casted;
            if (QueryEntitySafeDictionary(entityGID.groupID, out casted) == false) return false;

            return casted != null && casted.ContainsKey(entityGID.entityID);
        }

        public bool HasAny<T>() where T : IEntityStruct
        {
            return HasAny<T>(ExclusiveGroup.StandardEntitiesGroup);
        }

        public bool HasAny<T>(int @group) where T : IEntityStruct
        {
            int count;
            QueryEntities<T>(group, out count);
            return count > 0;
        }

        public bool TryQueryEntityView<T>(EGID entityegid, out T entityView) where T : class, IEntityStruct
        {
            return TryQueryEntityViewInGroupInternal(entityegid, out entityView);
        }
        
        bool TryQueryEntityViewInGroupInternal<T>(EGID entityGID, out T entityView) where T:class, IEntityStruct
        {
            entityView = null;
            TypeSafeDictionary<T> safeDictionary;
            if (QueryEntitySafeDictionary(entityGID.groupID, out safeDictionary) == false) return false;

            return safeDictionary.TryGetValue(entityGID.entityID, out entityView) != false;
        }

        T[] QueryEntitiesAndIndexInternal<T>(EGID entityGID, out uint index) where T : IEntityStruct
        {
            TypeSafeDictionary<T> safeDictionary;
            if (QueryEntitySafeDictionary(entityGID.groupID, out safeDictionary) == false)
                throw new EntitiesDBException("Entity not found, type: ".FastConcat(typeof(T)).FastConcat(" groupID: ").FastConcat(entityGID.entityID));

            if (safeDictionary.TryFindElementIndex(entityGID.entityID, out index) == false)
                throw new EntitiesDBException("Entity not found, type: ".FastConcat(typeof(T)).FastConcat(" groupID: ").FastConcat(entityGID.entityID));

            int count;
            return safeDictionary.GetFasterValuesBuffer(out count);
        }
        
        bool QueryEntitySafeDictionary<T>(int @group, out TypeSafeDictionary<T> typeSafeDictionary) where T : IEntityStruct
        {
            Dictionary<Type, ITypeSafeDictionary> entitiesInGroupPerType;
            typeSafeDictionary = null;

            //search for the group 
            if (_groupEntityViewsDB.TryGetValue(@group, out entitiesInGroupPerType) == false)
                return false;

            //search for the indexed entities in the group
            ITypeSafeDictionary safeDictionary;
            if (entitiesInGroupPerType.TryGetValue(typeof(T), out safeDictionary) == false)
                return false;

            //return the indexes entities if they exist
            typeSafeDictionary = (safeDictionary as TypeSafeDictionary<T>);
            
            return true;
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
        //needed to be able to iterate over all the entities of the same type regardless the group
        //may change in future
        readonly Dictionary<Type, FasterDictionary<int, ITypeSafeDictionary>> _groupedGroups;
    }
}
