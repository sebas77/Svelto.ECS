using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;

namespace Svelto.ECS.Internal
{
    partial class EntitiesDB : IEntitiesDB
    {
        internal EntitiesDB(Dictionary<int, Dictionary<Type, ITypeSafeDictionary>> groupEntityViewsDB,
            Dictionary<Type, FasterDictionary<int, ITypeSafeDictionary>> groupedGroups)
        {
            _groupEntityViewsDB = groupEntityViewsDB;
            _groupedGroups = groupedGroups;
        }

        public ReadOnlyCollectionStruct<T> QueryEntityViews<T>(int @group) where T:class, IEntityStruct
        {
            TypeSafeDictionary<T> typeSafeDictionary;
            if (QueryEntitySafeDictionary(@group, out typeSafeDictionary) == false) return RetrieveEmptyEntityViewList<T>();

            return typeSafeDictionary.FasterValues;
        }

        public T[] QueryEntities<T>(int @group, out int count) where T : IEntityStruct
        {
            TypeSafeDictionary<T> typeSafeDictionary;
            count = 0;
            if (QueryEntitySafeDictionary(@group, out typeSafeDictionary) == false) return RetrieveEmptyEntityViewArray<T>();

            return typeSafeDictionary.GetFasterValuesBuffer(out count);
        }

        public T[] QueryEntities<T>(ExclusiveGroup @group, out int targetsCount) where T : IEntityStruct
        {
            return QueryEntities<T>((int) @group, out targetsCount);
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

        public EGIDMapper<T> QueryMappedEntities<T>(ExclusiveGroup groupID) where T : IEntityStruct
        {
            return QueryMappedEntities<T>((int) groupID);
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

        public bool Exists<T>(EGID entityGID) where T : IEntityStruct
        {
            TypeSafeDictionary<T> casted;
            if (QueryEntitySafeDictionary(entityGID.groupID, out casted) == false) return false;

            return casted != null && casted.ContainsKey(entityGID.entityID);
        }

        public bool HasAny<T>(int @group) where T : IEntityStruct
        {
            int count;
            QueryEntities<T>(group, out count);
            return count > 0;
        }

        public bool HasAny<T>(ExclusiveGroup @group) where T : IEntityStruct
        {
            return HasAny<T>((int) group);
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
        
        static void SafetyChecks<T>(TypeSafeDictionary<T> typeSafeDictionary, int count) where T : IEntityStruct
        {
#if DEBUG            
            if (typeSafeDictionary.Count != count)
                throw new EntitiesDBException("Entities cannot be swapped or removed during an iteration");
#endif            
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
