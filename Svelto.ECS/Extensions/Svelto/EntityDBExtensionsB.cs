using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public static class EntityDBExtensionsB
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MB<T> QueryEntitiesAndIndex<T>(this EntitiesDB entitiesDb, EGID entityGID, out uint index) where T : struct, IEntityViewComponent
        {
            if (entitiesDb.QueryEntitiesAndIndexInternal<T>(entityGID, out index, out MB<T> array) == true)
                return array;

            throw new EntityNotFoundException(entityGID, typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryQueryEntitiesAndIndex<T>(this EntitiesDB entitiesDb, EGID entityGID, out uint index, out MB<T> array)
            where T : struct, IEntityViewComponent
        {
            if (entitiesDb.QueryEntitiesAndIndexInternal<T>(entityGID, out index, out array) == true)
                return true;

            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryQueryEntitiesAndIndex<T>(this EntitiesDB entitiesDb, uint id, ExclusiveGroupStruct group, out uint index, out MB<T> array)
            where T : struct, IEntityViewComponent
        {
            if (entitiesDb.QueryEntitiesAndIndexInternal<T>(new EGID(id, group), out index, out array) == true)
                return true;

            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool QueryEntitiesAndIndexInternal<T>(this EntitiesDB entitiesDb, EGID entityGID, out uint index, out MB<T> buffer) where T : struct, IEntityViewComponent
        {
            index  = 0;
            buffer = default;
            if (entitiesDb.SafeQueryEntityDictionary<T>(entityGID.groupID, out var safeDictionary) == false)
                return false;

            if (safeDictionary.TryFindIndex(entityGID.entityID, out index) == false)
                return false;
            
            buffer = (MB<T>) (safeDictionary as ITypeSafeDictionary<T>).GetValues(out _);

            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T QueryEntity<T>(this EntitiesDB entitiesDb, EGID entityGID) where T : struct, IEntityViewComponent
        {
            var array = entitiesDb.QueryEntitiesAndIndex<T>(entityGID, out var index);
           
            return ref array[(int) index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T QueryEntity<T>(this EntitiesDB entitiesDb, uint id, ExclusiveGroupStruct group) where T : struct, IEntityViewComponent
        {
            return ref entitiesDb.QueryEntity<T>(new EGID(id, group));
        }
    }
}