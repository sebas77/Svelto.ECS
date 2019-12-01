#if DEBUG && !PROFILER
#define ENABLE_DEBUG_FUNC
#endif

using System;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    partial class EntitiesDB : IEntitiesDB
    {
        internal EntitiesDB(
            FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> groupEntityViewsDB,
            FasterDictionary<RefWrapper<Type>, FasterDictionary<uint, ITypeSafeDictionary>> groupsPerEntity,
            EntitiesStream entityStream)
        {
            _groupEntityViewsDB = groupEntityViewsDB;
            _groupsPerEntity = groupsPerEntity;
            _entityStream = entityStream;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T QueryUniqueEntity<T>(ExclusiveGroup.ExclusiveGroupStruct group) where T : struct, IEntityStruct
        {
            var entities = QueryEntities<T>(group, out var count);

            if (count == 0)
                throw new ECSException("Unique entity not found '".FastConcat(typeof(T).ToString()).FastConcat("'"));
            if (count != 1)
                throw new ECSException("Unique entities must be unique! '".FastConcat(typeof(T).ToString())
                    .FastConcat("'"));
            return ref entities[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T QueryEntity<T>(EGID entityGID) where T : struct, IEntityStruct
        {
            T[] array;
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGID, out var index)) != null)
                return ref array[index];

            throw new EntityNotFoundException(entityGID, typeof(T));
        }

        public ref T QueryEntity<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group) where T : struct, IEntityStruct
        {
            return ref QueryEntity<T>(new EGID(id, group));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] QueryEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out uint count)
            where T : struct, IEntityStruct
        {
            uint group = groupStruct;
            count = 0;
            if (SafeQueryEntityDictionary(group, out TypeSafeDictionary<T> typeSafeDictionary) == false)
                return RetrieveEmptyEntityViewArray<T>();

            return typeSafeDictionary.GetValuesArray(out count);
        }

        public EntityCollection<T> QueryEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct)
            where T : struct, IEntityStruct
        {
            return new EntityCollection<T>(QueryEntities<T>(groupStruct, out var count), count);
        }

        public EntityCollection<T1, T2> QueryEntities<T1, T2>(ExclusiveGroup.ExclusiveGroupStruct groupStruct)
            where T1 : struct, IEntityStruct where T2 : struct, IEntityStruct
        {
            return new EntityCollection<T1, T2>(QueryEntities<T1, T2>(groupStruct, out var count), count);
        }

        public EntityCollection<T1, T2, T3> QueryEntities<T1, T2, T3>(ExclusiveGroup.ExclusiveGroupStruct groupStruct)
            where T1 : struct, IEntityStruct where T2 : struct, IEntityStruct where T3 : struct, IEntityStruct
        {
            return new EntityCollection<T1, T2, T3>(QueryEntities<T1, T2, T3>(groupStruct, out var count), count);
        }

        public EntityCollections<T> QueryEntities<T>(ExclusiveGroup[] groups) where T : struct, IEntityStruct
        {
            return new EntityCollections<T>(this, groups);
        }

        public EntityCollections<T1, T2> QueryEntities<T1, T2>(ExclusiveGroup[] groups)
            where T1 : struct, IEntityStruct where T2 : struct, IEntityStruct
        {
            return new EntityCollections<T1, T2>(this, groups);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (T1[], T2[]) QueryEntities<T1, T2>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out uint count)
            where T1 : struct, IEntityStruct
            where T2 : struct, IEntityStruct
        {
            var T1entities = QueryEntities<T1>(groupStruct, out var countCheck);
            var T2entities = QueryEntities<T2>(groupStruct, out count);

            if (count != countCheck)
            {
                throw new ECSException("Entity views count do not match in group. Entity 1: ' count: "
                    .FastConcat(countCheck)
                    .FastConcat(typeof(T1).ToString())
                    .FastConcat("'. Entity 2: ' count: ".FastConcat(count)
                        .FastConcat(typeof(T2).ToString())
                        .FastConcat("'")));
            }

            return (T1entities, T2entities);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (T1[], T2[], T3[]) QueryEntities
            <T1, T2, T3>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out uint count)
            where T1 : struct, IEntityStruct where T2 : struct, IEntityStruct where T3 : struct, IEntityStruct
        {
            var T1entities = QueryEntities<T1>(groupStruct, out var countCheck1);
            var T2entities = QueryEntities<T2>(groupStruct, out var countCheck2);
            var T3entities = QueryEntities<T3>(groupStruct, out count);

            if (count != countCheck1 || count != countCheck2)
                throw new ECSException("Entity views count do not match in group. Entity 1: "
                    .FastConcat(typeof(T1).ToString()).FastConcat(" count: ").FastConcat(countCheck1).FastConcat(
                        " Entity 2: ".FastConcat(typeof(T2).ToString())
                            .FastConcat(" count: ").FastConcat(countCheck2)
                            .FastConcat(" Entity 3: ".FastConcat(typeof(T3).ToString())).FastConcat(" count: ")
                            .FastConcat(count)));

            return (T1entities, T2entities, T3entities);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EGIDMapper<T> QueryMappedEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId)
            where T : struct, IEntityStruct
        {
            if (SafeQueryEntityDictionary(groupStructId, out TypeSafeDictionary<T> typeSafeDictionary) == false)
                throw new EntityGroupNotFoundException(groupStructId, typeof(T));

            EGIDMapper<T> mapper;
            mapper.map = typeSafeDictionary;

            return mapper;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryMappedEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId,
            out EGIDMapper<T> mapper)
            where T : struct, IEntityStruct
        {
            mapper = default;
            if (SafeQueryEntityDictionary(groupStructId, out TypeSafeDictionary<T> typeSafeDictionary) == false)
                return false;

            mapper.map = typeSafeDictionary;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] QueryEntitiesAndIndex<T>(EGID entityGID, out uint index) where T : struct, IEntityStruct
        {
            T[] array;
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGID, out index)) != null)
                return array;

            throw new EntityNotFoundException(entityGID, typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryEntitiesAndIndex<T>(EGID entityGid, out uint index, out T[] array)
            where T : struct, IEntityStruct
        {
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGid, out index)) != null)
                return true;

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] QueryEntitiesAndIndex<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group, out uint index)
            where T : struct, IEntityStruct
        {
            return QueryEntitiesAndIndex<T>(new EGID(id, group), out index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryEntitiesAndIndex<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group, out uint index,
            out T[] array) where T : struct, IEntityStruct
        {
            return TryQueryEntitiesAndIndex(new EGID(id, group), out index, out array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists<T>(EGID entityGID) where T : struct, IEntityStruct
        {
            if (SafeQueryEntityDictionary(entityGID.groupID, out TypeSafeDictionary<T> casted) == false) return false;

            return casted != null && casted.ContainsKey(entityGID.entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group) where T : struct, IEntityStruct
        {
            if (SafeQueryEntityDictionary(group, out TypeSafeDictionary<T> casted) == false) return false;

            return casted != null && casted.ContainsKey(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(ExclusiveGroup.ExclusiveGroupStruct gid)
        {
            return _groupEntityViewsDB.ContainsKey(gid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAny<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct) where T : struct, IEntityStruct
        {
            QueryEntities<T>(groupStruct, out var count);
            return count > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Count<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct) where T : struct, IEntityStruct
        {
            QueryEntities<T>(groupStruct, out var count);
            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PublishEntityChange<T>(EGID egid) where T : unmanaged, IEntityStruct
        {
            _entityStream.PublishEntity(ref QueryEntity<T>(egid), egid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        T[] QueryEntitiesAndIndexInternal<T>(EGID entityGID, out uint index) where T : struct, IEntityStruct
        {
            index = 0;
            if (SafeQueryEntityDictionary(entityGID.groupID, out TypeSafeDictionary<T> safeDictionary) == false)
                return null;

            if (safeDictionary.TryFindIndex(entityGID.entityID, out index) == false)
                return null;

            return safeDictionary.GetValuesArray(out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool SafeQueryEntityDictionary<T>(uint group, out TypeSafeDictionary<T> typeSafeDictionary)
            where T : struct, IEntityStruct
        {
            if (UnsafeQueryEntityDictionary(group, TypeCache<T>.type, out var safeDictionary) == false)
            {
                typeSafeDictionary = default;
                return false;
            }

            //return the indexes entities if they exist
            typeSafeDictionary = safeDictionary as TypeSafeDictionary<T>;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool UnsafeQueryEntityDictionary(uint group, Type type, out ITypeSafeDictionary typeSafeDictionary)
        {
            //search for the group
            if (_groupEntityViewsDB.TryGetValue(group, out var entitiesInGroupPerType) == false)
            {
                typeSafeDictionary = null;
                return false;
            }

            //search for the indexed entities in the group
            return entitiesInGroupPerType.TryGetValue(new RefWrapper<Type>(type), out typeSafeDictionary);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T[] RetrieveEmptyEntityViewArray<T>()
        {
            return EmptyList<T>.emptyArray;
        }

        //grouped set of entity views, this is the standard way to handle entity views entity views are grouped per
        //group, then indexable per type, then indexable per EGID. however the TypeSafeDictionary can return an array of
        //values directly, that can be iterated over, so that is possible to iterate over all the entity views of
        //a specific type inside a specific group.
        readonly FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> _groupEntityViewsDB;

        //needed to be able to iterate over all the entities of the same type regardless the group
        //may change in future
        readonly FasterDictionary<RefWrapper<Type>, FasterDictionary<uint, ITypeSafeDictionary>> _groupsPerEntity;
        readonly EntitiesStream                                                                  _entityStream;

        static class EmptyList<T>
        {
            internal static readonly T[] emptyArray = new T[0];
        }
    }
}