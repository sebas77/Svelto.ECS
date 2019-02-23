#if DEBUG && !PROFILER
#define ENABLE_DEBUG_FUNC
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;

namespace Svelto.ECS.Internal
{
    partial class EntitiesDB : IEntitiesDB
    {
        internal EntitiesDB(FasterDictionary<int, Dictionary<Type, ITypeSafeDictionary>> groupEntityViewsDB,
            Dictionary<Type, FasterDictionary<int, ITypeSafeDictionary>> groupedGroups)
        {
            _groupEntityViewsDB = groupEntityViewsDB;
            _groupedGroups = groupedGroups;
        }

        public ReadOnlyCollectionStruct<T> QueryEntityViews<T>(int group) where T:class, IEntityStruct
        {
            if (QueryEntitySafeDictionary(group, out TypeSafeDictionary<T> typeSafeDictionary) == false) 
                return new ReadOnlyCollectionStruct<T>(RetrieveEmptyEntityViewArray<T>(), 0);

            return typeSafeDictionary.Values;
        }

        public ReadOnlyCollectionStruct<T> QueryEntityViews<T>(ExclusiveGroup.ExclusiveGroupStruct group) where T : class, IEntityStruct
        {
            return QueryEntityViews<T>((int) group);
        }

        public T QueryEntityView<T>(int id, ExclusiveGroup.ExclusiveGroupStruct group) where T : class, IEntityStruct
        {
            return QueryEntityView<T>(new EGID(id, (int) group));
        }

        public ref T QueryUniqueEntity<T>(int @group) where T : IEntityStruct
        {
            var entities = QueryEntities<T>(group, out var count);
            
            if (count != 1) throw new ECSException("Unique entities must be unique!".FastConcat(typeof(T).ToString()));
            
            return ref entities[0];
        }
        
        public ref T QueryUniqueEntity<T>(ExclusiveGroup.ExclusiveGroupStruct @group) where T : IEntityStruct
        {
            return ref QueryUniqueEntity<T>((int) @group);
        }

        public ref T QueryEntity<T>(EGID entityGID) where T : IEntityStruct
        {
            T[]  array;
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGID, out var index)) != null)
                return ref array[index];
            
            throw new EntityNotFoundException(entityGID.entityID, entityGID.groupID, typeof(T));
        }
        
        public ref T QueryEntity<T>(int id, ExclusiveGroup.ExclusiveGroupStruct group) where T : IEntityStruct
        {
            return ref QueryEntity<T>(new EGID(id, group));
        }

        public ref T QueryEntity<T>(int id, int group) where T : IEntityStruct
        {
            return ref QueryEntity<T>(new EGID(id, group));
        }

        public T[] QueryEntities<T>(int group, out int count) where T : IEntityStruct
        {
            count = 0;
            if (QueryEntitySafeDictionary(group, out TypeSafeDictionary<T> typeSafeDictionary) == false) 
                return RetrieveEmptyEntityViewArray<T>();

            return typeSafeDictionary.GetValuesArray(out count);
        }

        public T[] QueryEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out int count) where T : IEntityStruct
        {
            return QueryEntities<T>((int) groupStruct, out count);
        }

        public (T1[], T2[]) QueryEntities<T1, T2>(int @group, out int count) where T1 : IEntityStruct where T2 : IEntityStruct
        {
            var T1entities = QueryEntities<T1>(group, out var countCheck);
            var T2entities = QueryEntities<T2>(group, out count);
            
            if (count != countCheck) 
                throw new ECSException("Entity views count do not match in group. Entity 1: ".
                                       FastConcat(typeof(T1).ToString()).FastConcat(
                                       "Entity 2: ".FastConcat(typeof(T2).ToString())));

            
            return (T1entities, T2entities);
        }

        public (T1[], T2[]) QueryEntities<T1, T2>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out int count) where T1 : IEntityStruct where T2 : IEntityStruct
        {
            return QueryEntities<T1, T2>((int) groupStruct, out count);
        }

        public EGIDMapper<T> QueryMappedEntities<T>(int groupID) where T : IEntityStruct
        {
            TypeSafeDictionary<T> typeSafeDictionary;
            
            if (QueryEntitySafeDictionary(groupID, out typeSafeDictionary) == false) 
                throw new EntityGroupNotFoundException(groupID, typeof(T));

            EGIDMapper<T> mapper;
            mapper.map = typeSafeDictionary;

            int count;
            typeSafeDictionary.GetValuesArray(out count);

            return mapper;
        }

        public EGIDMapper<T> QueryMappedEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId) where T : IEntityStruct
        {
            return QueryMappedEntities<T>((int) groupStructId);
        }

        public T[] QueryEntitiesAndIndex<T>(EGID entityGID, out uint index) where T : IEntityStruct
        {
            T[] array;
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGID, out index)) != null)
                return array;
            
            throw new EntityNotFoundException(entityGID.entityID, entityGID.groupID, typeof(T)); 
        }
        
        public bool TryQueryEntitiesAndIndex<T>(EGID entityGid, out uint index, out T[] array) where T : IEntityStruct
        {
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGid, out index)) != null)
                return true;
            
            return false;
        }

        public T[] QueryEntitiesAndIndex<T>(int id, ExclusiveGroup.ExclusiveGroupStruct group, out uint index) where T : IEntityStruct
        {
            return QueryEntitiesAndIndex<T>(new EGID(id, group), out index);
        }

        public bool TryQueryEntitiesAndIndex<T>(int id, ExclusiveGroup.ExclusiveGroupStruct group, out uint index, out T[] array) where T : IEntityStruct
        {
            return TryQueryEntitiesAndIndex(new EGID(id, group), out index, out array);
        }

        public T[] QueryEntitiesAndIndex<T>(int id, int group, out uint index) where T : IEntityStruct
        {
            return QueryEntitiesAndIndex<T>(new EGID(id, group), out index);
        }

        public bool TryQueryEntitiesAndIndex<T>(int id, int group, out uint index, out T[] array) where T : IEntityStruct
        {
            return TryQueryEntitiesAndIndex(new EGID(id, group), out index, out array);
        }

        public T QueryEntityView<T>(EGID entityGID) where T : class, IEntityStruct
        {
            T entityView;

            if (TryQueryEntityViewInGroupInternal(entityGID, out entityView) == false)
                throw new EntityNotFoundException(entityGID.entityID, entityGID.groupID, typeof(T));

            return entityView;
        }

        public bool Exists<T>(EGID entityGID) where T : IEntityStruct
        {
            TypeSafeDictionary<T> casted;
            if (QueryEntitySafeDictionary(entityGID.groupID, out casted) == false) return false;

            return casted != null && casted.ContainsKey(entityGID.entityID);
        }

        public bool Exists<T>(int id, int groupid) where T : IEntityStruct
        {
            return Exists<T>(new EGID(id, groupid));
        }

        //search for the group 
        public bool Exists(ExclusiveGroup.ExclusiveGroupStruct gid)
        {
            return _groupEntityViewsDB.ContainsKey(gid);
        }

        public bool HasAny<T>(int group) where T : IEntityStruct
        {
            int count;
            QueryEntities<T>(group, out count);
            return count > 0;
        }

        public bool HasAny<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct) where T : IEntityStruct
        {
            return HasAny<T>((int) groupStruct);
        }

        public int Count<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct) where T : IEntityStruct
        {
            int count;
            QueryEntities<T>(groupStruct, out count);
            return count;
        }

        public int Count<T>(int groupStruct) where T : IEntityStruct
        {
            int count;
            QueryEntities<T>(groupStruct, out count);
            return count;
        }

        public bool TryQueryEntityView<T>(EGID entityegid, out T entityView) where T : class, IEntityStruct
        {
            return TryQueryEntityViewInGroupInternal(entityegid, out entityView);
        }
        
        public bool TryQueryEntityView<T>(int id, ExclusiveGroup.ExclusiveGroupStruct group, out T entityView) where T : class, IEntityStruct
        {
            return TryQueryEntityViewInGroupInternal(new EGID(id, (int) group), out entityView);
        }
        
        bool TryQueryEntityViewInGroupInternal<T>(EGID entityGID, out T entityView) where T:class, IEntityStruct
        {
            entityView = null;
            TypeSafeDictionary<T> safeDictionary;
            if (QueryEntitySafeDictionary(entityGID.groupID, out safeDictionary) == false) return false;

            return safeDictionary.TryGetValue(entityGID.entityID, out entityView);
        }

        T[] QueryEntitiesAndIndexInternal<T>(EGID entityGID, out uint index) where T : IEntityStruct
        {
            TypeSafeDictionary<T> safeDictionary;
            index = 0;
            if (QueryEntitySafeDictionary(entityGID.groupID, out safeDictionary) == false)
                return null;

            if (safeDictionary.TryFindElementIndex(entityGID.entityID, out index) == false)
                return null;

            int count;
            return safeDictionary.GetValuesArray(out count);
        }
        
        bool QueryEntitySafeDictionary<T>(int group, out TypeSafeDictionary<T> typeSafeDictionary) where T : IEntityStruct
        {
            Dictionary<Type, ITypeSafeDictionary> entitiesInGroupPerType;
            typeSafeDictionary = null;

            //search for the group 
            if (_groupEntityViewsDB.TryGetValue(group, out entitiesInGroupPerType) == false)
                return false;

            //search for the indexed entities in the group
            ITypeSafeDictionary safeDictionary;
            if (entitiesInGroupPerType.TryGetValue(typeof(T), out safeDictionary) == false)
                return false;

            //return the indexes entities if they exist
            typeSafeDictionary = (safeDictionary as TypeSafeDictionary<T>);
            
            return true;
        }
        
        [Conditional("ENABLE_DEBUG_FUNC")]
        static void SafetyChecks<T>(TypeSafeDictionary<T> typeSafeDictionary, int count) where T : IEntityStruct
        {
            if (typeSafeDictionary.Count != count)
                throw new ECSException("Entities cannot be swapped or removed during an iteration");
        }

        static ReadOnlyCollectionStruct<T> RetrieveEmptyEntityViewList<T>()
        {
            var arrayFast = FasterList<T>.DefaultList.ToArrayFast();
            
            return new ReadOnlyCollectionStruct<T>(arrayFast, 0);
        }

        static T[] RetrieveEmptyEntityViewArray<T>()
        {
            return FasterList<T>.DefaultList.ToArrayFast();
        }
     
        //grouped set of entity views, this is the standard way to handle entity views entity views are grouped per
        //group, then indexable per type, then indexable per EGID. however the TypeSafeDictionary can return an array of
        //values directly, that can be iterated over, so that is possible to iterate over all the entity views of 
        //a specific type inside a specific group.
        readonly FasterDictionary<int, Dictionary<Type, ITypeSafeDictionary>> _groupEntityViewsDB;
        //needed to be able to iterate over all the entities of the same type regardless the group
        //may change in future
        readonly Dictionary<Type, FasterDictionary<int, ITypeSafeDictionary>> _groupedGroups;
    }
}
