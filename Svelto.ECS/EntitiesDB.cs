#if DEBUG && !PROFILE_SVELTO
#define ENABLE_DEBUG_FUNC
#endif

using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public class EntitiesDB
    {
        internal EntitiesDB(
            FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> groupEntityComponentsDB,
            FasterDictionary<RefWrapper<Type>, FasterDictionary<uint, ITypeSafeDictionary>> groupsPerEntity,
            EntitiesStream entityStream)
        {
            _groupEntityComponentsDB = groupEntityComponentsDB;
            _groupsPerEntity = groupsPerEntity;
            _entityStream = entityStream;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T QueryUniqueEntity<T>(ExclusiveGroupStruct group) where T : struct, IEntityComponent
        {
            var entities = QueryEntities<T>(group).ToFastAccess(out var count);

#if DEBUG && !PROFILE_SVELTO
            if (count == 0)
                throw new ECSException("Unique entity not found '".FastConcat(typeof(T).ToString()).FastConcat("'"));
            if (count != 1)
                throw new ECSException("Unique entities must be unique! '".FastConcat(typeof(T).ToString())
                    .FastConcat("'"));
#endif
            return ref entities[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T QueryEntity<T>(EGID entityGID) where T : struct, IEntityComponent
        {
            T[] array;
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGID, out var index)) != null)
                return ref array[(int) index];

            throw new EntityNotFoundException(entityGID, typeof(T));
        }

        public ref T QueryEntity<T>(uint id, ExclusiveGroupStruct group) where T : struct, IEntityComponent
        {
            return ref QueryEntity<T>(new EGID(id, group));
        }

        /// <summary>
        /// The QueryEntities<T> follows the rule that entities could always be iterated regardless if they
        /// are 0, 1 or N. In case of 0 it returns an empty array. This allows to use the same for iteration
        /// regardless the number of entities built.
        /// </summary>
        /// <param name="groupStructId"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public EntityCollection<T> QueryEntities<T>(ExclusiveGroupStruct groupStructId)
            where T : struct, IEntityComponent
        {
            T[] ret;
            uint count = 0;
            //object sentinel = default;
            if (SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false)
                ret = RetrieveEmptyEntityComponentArray<T>();
            else
            {
                var safeDictionary = (typeSafeDictionary as ITypeSafeDictionary<T>);
                ret = safeDictionary.GetValuesArray(out count);
              //  sentinel = safeDictionary.GenerateSentinel();
            }

            return new EntityCollection<T>(ret, count);
        }

        public EntityCollection<T1, T2> QueryEntities<T1, T2>(
            ExclusiveGroupStruct groupStruct)
            where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent
        {
            var T1entities = QueryEntities<T1>(groupStruct);
            var T2entities = QueryEntities<T2>(groupStruct);

            if (T1entities.count != T2entities.count)
                throw new ECSException("Entity views count do not match in group. Entity 1: ' count: "
                    .FastConcat(T1entities.count).FastConcat(typeof(T1).ToString())
                    .FastConcat("'. Entity 2: ' count: ".FastConcat(T2entities.count)
                        .FastConcat(typeof(T2).ToString())
                        .FastConcat("'")));

            return new EntityCollection<T1, T2>(T1entities, T2entities);
        }

        public EntityCollection<T1, T2, T3>
            QueryEntities<T1, T2, T3>(ExclusiveGroupStruct groupStruct)
            where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent where T3 : struct, IEntityComponent
        {
            var T1entities = QueryEntities<T1>(groupStruct);
            var T2entities = QueryEntities<T2>(groupStruct);
            var T3entities = QueryEntities<T3>(groupStruct);

            if (T1entities.count != T2entities.count || T2entities.count != T3entities.count)
                throw new ECSException("Entity views count do not match in group. Entity 1: "
                    .FastConcat(typeof(T1).ToString()).FastConcat(" count: ")
                    .FastConcat(T1entities.count)
                    .FastConcat(" Entity 2: "
                        .FastConcat(typeof(T2).ToString()).FastConcat(" count: ")
                        .FastConcat(T2entities.count)
                        .FastConcat(" Entity 3: ".FastConcat(typeof(T3).ToString()))
                        .FastConcat(" count: ").FastConcat(T3entities.count)));

            return new EntityCollection<T1, T2, T3>(T1entities,
                T2entities, T3entities);
        }

        public EntityCollections<T> QueryEntities<T>(ExclusiveGroup[] groups) where T : struct, IEntityComponent
        {
            return new EntityCollections<T>(this, groups);
        }

        public EntityCollections<T1, T2> QueryEntities<T1, T2>(ExclusiveGroup[] groups)
            where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent
        {
            return new EntityCollections<T1, T2>(this, groups);
        }
        
        public EntityCollections<T1, T2, T3> QueryEntities<T1, T2, T3>(ExclusiveGroup[] groups)
            where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent where T3 : struct, IEntityComponent
        {
            return new EntityCollections<T1, T2, T3>(this, groups);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EGIDMapper<T> QueryMappedEntities<T>(ExclusiveGroupStruct groupStructId)
            where T : struct, IEntityComponent
        {
            if (SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false)
                throw new EntityGroupNotFoundException(typeof(T));

            return (typeSafeDictionary as ITypeSafeDictionary<T>).ToEGIDMapper(groupStructId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeEGIDMapper<T> QueryNativeMappedEntities<T>(ExclusiveGroupStruct groupStructId)
            where T : unmanaged, IEntityComponent
        {
            if (SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false)
                throw new EntityGroupNotFoundException(typeof(T));

            return (typeSafeDictionary as TypeSafeDictionary<T>).ToNativeEGIDMapper<T>(groupStructId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryMappedEntities<T>(ExclusiveGroupStruct groupStructId,
            out EGIDMapper<T> mapper)
            where T : struct, IEntityComponent
        {
            mapper = default;
            if (SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false ||
                typeSafeDictionary.Count == 0)
                return false;

            mapper = (typeSafeDictionary as ITypeSafeDictionary<T>).ToEGIDMapper(groupStructId);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryNativeMappedEntities<T>(ExclusiveGroupStruct groupStructId,
            out NativeEGIDMapper<T> mapper)
            where T : unmanaged, IEntityComponent
        {
            mapper = default;
            if (SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false ||
                typeSafeDictionary.Count == 0)
                return false;

            mapper = (typeSafeDictionary as TypeSafeDictionary<T>).ToNativeEGIDMapper(groupStructId);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] QueryEntitiesAndIndex<T>(EGID entityGID, out uint index) where T : struct, IEntityComponent
        {
            T[] array;
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGID, out index)) != null)
                return array;

            throw new EntityNotFoundException(entityGID, typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryEntitiesAndIndex<T>(EGID entityGid, out uint index, out T[] array)
            where T : struct, IEntityComponent
        {
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGid, out index)) != null)
                return true;

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] QueryEntitiesAndIndex<T>(uint id, ExclusiveGroupStruct @group, out uint index)
            where T : struct, IEntityComponent
        {
            return QueryEntitiesAndIndex<T>(new EGID(id, @group), out index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryEntitiesAndIndex
            <T>(uint id, ExclusiveGroupStruct group, out uint index, out T[] array)
            where T : struct, IEntityComponent
        {
            return TryQueryEntitiesAndIndex(new EGID(id, @group), out index, out array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists<T>(EGID entityGID) where T : struct, IEntityComponent
        {
            if (SafeQueryEntityDictionary<T>(entityGID.groupID, out var casted) == false) return false;

            return casted != null && casted.ContainsKey(entityGID.entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists<T>(uint id, ExclusiveGroupStruct group) where T : struct, IEntityComponent
        {
            if (SafeQueryEntityDictionary<T>(group, out var casted) == false) return false;

            return casted != null && casted.ContainsKey(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExistsAndIsNotEmpty(ExclusiveGroupStruct gid)
        {
            if (_groupEntityComponentsDB.TryGetValue(gid,
                    out FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> group) == true)
            {
                return group.count > 0;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAny<T>(ExclusiveGroupStruct groupStruct) where T : struct, IEntityComponent
        {
            return QueryEntities<T>(groupStruct).count > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Count<T>(ExclusiveGroupStruct groupStruct) where T : struct, IEntityComponent
        {
            return QueryEntities<T>(groupStruct).count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PublishEntityChange<T>(EGID egid) where T : unmanaged, IEntityComponent
        {
            _entityStream.PublishEntity(ref QueryEntity<T>(egid), egid);
        }

        public void ExecuteOnAllEntities<T>(ExecuteOnAllEntitiesAction<T> action) where T : struct, IEntityComponent
        {
            if (_groupsPerEntity.TryGetValue(TypeRefWrapper<T>.wrapper, out var dictionary))
                foreach (var pair in dictionary)
                {
                    var entities = (pair.Value as ITypeSafeDictionary<T>).GetValuesArray(out var count);

                    if (count > 0)
                        action(entities, new ExclusiveGroupStruct(pair.Key), count, this);
                }
        }

        public void ExecuteOnAllEntities<T, W>(ref W value, ExecuteOnAllEntitiesAction<T, W> action)
            where T : struct, IEntityComponent
        {
            if (_groupsPerEntity.TryGetValue(TypeRefWrapper<T>.wrapper, out var dic))
                foreach (var pair in dic)
                {
                    var entities = (pair.Value as ITypeSafeDictionary<T>).GetValuesArray(out var innerCount);

                    if (innerCount > 0)
                        action(entities, new ExclusiveGroupStruct(pair.Key), innerCount, this,
                            ref value);
                }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        T[] QueryEntitiesAndIndexInternal<T>(EGID entityGID, out uint index) where T : struct, IEntityComponent
        {
            index = 0;
            if (SafeQueryEntityDictionary<T>(entityGID.groupID, out var safeDictionary) == false)
                return null;

            if (safeDictionary.TryFindIndex(entityGID.entityID, out index) == false)
                return null;

            return (safeDictionary as ITypeSafeDictionary<T>).unsafeValues;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool SafeQueryEntityDictionary<T>(uint group, out ITypeSafeDictionary typeSafeDictionary)
            where T : struct, IEntityComponent
        {
            if (UnsafeQueryEntityDictionary(group, TypeCache<T>.type, out var safeDictionary) == false)
            {
                typeSafeDictionary = default;
                return false;
            }

            //return the indexes entities if they exist
            typeSafeDictionary = safeDictionary;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool UnsafeQueryEntityDictionary(uint group, Type type, out ITypeSafeDictionary typeSafeDictionary)
        {
            //search for the group
            if (_groupEntityComponentsDB.TryGetValue(group, out var entitiesInGroupPerType) == false)
            {
                typeSafeDictionary = null;
                return false;
            }

            //search for the indexed entities in the group
            return entitiesInGroupPerType.TryGetValue(new RefWrapper<Type>(type), out typeSafeDictionary);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T[] RetrieveEmptyEntityComponentArray<T>()
        {
            return EmptyList<T>.emptyArray;
        }

        static class EmptyList<T>
        {
            internal static readonly T[] emptyArray = new T[0];
        }
        
        internal FasterDictionary<uint, ITypeSafeDictionary> FindGroups<T1>() where T1 : unmanaged, IEntityComponent
        {
            return _groupsPerEntity[TypeRefWrapper<T1>.wrapper];
        }
        
        readonly EntitiesStream _entityStream;

        //grouped set of entity views, this is the standard way to handle entity views entity views are grouped per
        //group, then indexable per type, then indexable per EGID. however the TypeSafeDictionary can return an array of
        //values directly, that can be iterated over, so that is possible to iterate over all the entity views of
        //a specific type inside a specific group.
        readonly FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> _groupEntityComponentsDB;

        //needed to be able to track in which groups a specific entity type can be found.
        //may change in future as it could be expanded to support queries
        readonly FasterDictionary<RefWrapper<Type>, FasterDictionary<uint, ITypeSafeDictionary>> _groupsPerEntity;
    }
}