using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

//todo: once using native memory for unmanaged struct will be optional, this will need to be moved under the Native namespace
namespace Svelto.ECS
{
    public static class EntityNativeDBExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NB<T> QueryEntitiesAndIndex<T>
            (this EntitiesDB entitiesDb, EGID entityGID, out uint index) where T : unmanaged, IEntityComponent
        {
            if (entitiesDb.QueryEntitiesAndIndexInternal(entityGID, out index, out NB<T> array) == true)
                return array;

            throw new EntityNotFoundException(entityGID, typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NB<T> QueryEntitiesAndIndex<T>
            (this EntitiesDB entitiesDb, uint id, ExclusiveGroupStruct group, out uint index)
            where T : unmanaged, IEntityComponent
        {
            EGID entityGID = new EGID(id, group);
            if (entitiesDb.QueryEntitiesAndIndexInternal(entityGID, out index, out NB<T> array) == true)
                return array;

            throw new EntityNotFoundException(entityGID, typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryQueryEntitiesAndIndex<T>
            (this EntitiesDB entitiesDb, EGID entityGID, out uint index, out NB<T> array)
            where T : unmanaged, IEntityComponent
        {
            if (entitiesDb.QueryEntitiesAndIndexInternal(entityGID, out index, out array) == true)
                return true;

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryQueryEntitiesAndIndex<T>
            (this EntitiesDB entitiesDb, uint id, ExclusiveGroupStruct group, out uint index, out NB<T> array)
            where T : unmanaged, IEntityComponent
        {
            if (entitiesDb.QueryEntitiesAndIndexInternal(new EGID(id, group), out index, out array) == true)
                return true;

            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetEntity<T>(this EntitiesDB entitiesDb, uint entityID, ExclusiveGroupStruct @group, out T value)
            where T : unmanaged, IEntityComponent
        {
            if (TryQueryEntitiesAndIndex<T>(entitiesDb, entityID, group, out var index, out var array))
            {
                value = array[index];
                return true;
            }

            value = default;
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetEntity<T>(this EntitiesDB entitiesDb, EGID egid, out T value)
            where T : unmanaged, IEntityComponent
        {
            return TryGetEntity<T>(entitiesDb, egid.entityID, egid.groupID, out value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool QueryEntitiesAndIndexInternal<T>
            (this EntitiesDB entitiesDb, EGID entityGID, out uint index, out NB<T> buffer)
            where T : unmanaged, IEntityComponent
        {
            index  = 0;
            buffer = default;
            if (entitiesDb.SafeQueryEntityDictionary<T>(entityGID.groupID, out var safeDictionary) == false)
                return false;

            if (safeDictionary.TryFindIndex(entityGID.entityID, out index) == false)
                return false;

            buffer = (NBInternal<T>) (safeDictionary as ITypeSafeDictionary<T>).GetValues(out _);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T QueryEntity<T>
            (this EntitiesDB entitiesDb, EGID entityGID) where T : unmanaged, IEntityComponent
        {
            var array = entitiesDb.QueryEntitiesAndIndex<T>(entityGID, out var index);

            return ref array[(int) index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T QueryEntity<T>
            (this EntitiesDB entitiesDb, uint id, ExclusiveGroupStruct group) where T : unmanaged, IEntityComponent
        {
            return ref entitiesDb.QueryEntity<T>(new EGID(id, group));
        }

        /// <summary>
        /// Expects that only one entity of type T exists in the group 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T QueryUniqueEntity<T>
            (this EntitiesDB entitiesDb, ExclusiveGroupStruct group) where T : unmanaged, IEntityComponent
        {
            var (entities, entitiescount) = entitiesDb.QueryEntities<T>(@group);

#if DEBUG && !PROFILE_SVELTO
            if (entitiescount == 0)
                throw new ECSException("Unique entity not found '".FastConcat(typeof(T).ToString()).FastConcat("'"));
            if (entitiescount != 1)
                throw new ECSException("Unique entities must be unique! '".FastConcat(typeof(T).ToString())
                                                                          .FastConcat("'"));
#endif
            return ref entities[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NB<T> GetArrayAndEntityIndex<T>
            (this EGIDMapper<T> mapper, uint entityID, out uint index) where T : unmanaged, IEntityComponent
        {
            if (mapper._map.TryFindIndex(entityID, out index))
            {
                return (NBInternal<T>) mapper._map.GetValues(out _);
            }

            throw new ECSException("Entity not found");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetArrayAndEntityIndex<T>
            (this EGIDMapper<T> mapper, uint entityID, out uint index, out NB<T> array)
            where T : unmanaged, IEntityComponent
        {
            index = default;
            if (mapper._map != null && mapper._map.TryFindIndex(entityID, out index))
            {
                array = (NBInternal<T>) mapper._map.GetValues(out _);
                return true;
            }

            array = default;
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AllGroupsEnumerable<T1> QueryEntities<T1>(this EntitiesDB db)
                where T1 :unmanaged, IEntityComponent
        {
            return new AllGroupsEnumerable<T1>(db);
        }
    }
}