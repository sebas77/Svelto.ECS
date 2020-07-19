using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public static class EntityDBExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static AllGroupsEnumerable<T1> QueryEntities<T1>(this EntitiesDB db)
            where T1 :struct, IEntityComponent
        {
            return new AllGroupsEnumerable<T1>(db);
        }
       
       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static NB<T> QueryEntitiesAndIndex<T>(this EntitiesDB entitiesDb, EGID entityGID, out uint index) where T : unmanaged, IEntityComponent
       {
           if (entitiesDb.QueryEntitiesAndIndexInternal<T>(entityGID, out index, out NB<T> array) == true)
               return array;

           throw new EntityNotFoundException(entityGID, typeof(T));
       }
       
       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static NB<T> QueryEntitiesAndIndex<T>(this EntitiesDB entitiesDb, uint id, ExclusiveGroupStruct group, out uint index) where T : unmanaged, IEntityComponent
       {
           EGID entityGID = new EGID(id, group);
           if (entitiesDb.QueryEntitiesAndIndexInternal<T>(entityGID, out index, out NB<T> array) == true)
               return array;

           throw new EntityNotFoundException(entityGID, typeof(T));
       }

       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static bool TryQueryEntitiesAndIndex<T>(this EntitiesDB entitiesDb, EGID entityGID, out uint index, out NB<T> array)
           where T : unmanaged, IEntityComponent
       {
           if (entitiesDb.QueryEntitiesAndIndexInternal<T>(entityGID, out index, out array) == true)
               return true;

           return false;
       }
       
       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static bool TryQueryEntitiesAndIndex<T>(this EntitiesDB entitiesDb, uint id, ExclusiveGroupStruct group, out uint index, out NB<T> array)
           where T : unmanaged, IEntityComponent
       {
           if (entitiesDb.QueryEntitiesAndIndexInternal<T>(new EGID(id, group), out index, out array) == true)
               return true;

           return false;
       }
        
       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       static bool QueryEntitiesAndIndexInternal<T>(this EntitiesDB entitiesDb, EGID entityGID, out uint index, out NB<T> buffer) where T : unmanaged, IEntityComponent
       {
           index = 0;
           buffer = default;
           if (entitiesDb.SafeQueryEntityDictionary<T>(entityGID.groupID, out var safeDictionary) == false)
               return false;

           if (safeDictionary.TryFindIndex(entityGID.entityID, out index) == false)
               return false;
            
           buffer = (NB<T>) (safeDictionary as ITypeSafeDictionary<T>).GetValues(out _);

           return true;
       }
       
       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static ref T QueryEntity<T>(this EntitiesDB entitiesDb, EGID entityGID) where T : unmanaged, IEntityComponent
       {
           var array = entitiesDb.QueryEntitiesAndIndex<T>(entityGID, out var index);
           
           return ref array[(int) index];
       }

       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static ref T QueryEntity<T>(this EntitiesDB entitiesDb, uint id, ExclusiveGroupStruct group) where T : unmanaged, IEntityComponent
       {
           return ref entitiesDb.QueryEntity<T>(new EGID(id, group));
       }
    }
}