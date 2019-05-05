using System;

namespace Svelto.ECS
{
    public interface IEntitiesDB
    {
        /// <summary>
        /// ECS is meant to work on a set of Entities. Working on a single entity is sometime necessary, but using
        /// the following functions inside a loop would be a mistake as performance can be significantly impacted
        /// return the buffer and the index of the entity inside the buffer using the input EGID
        /// </summary>
        /// <param name="entityGid"></param>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool TryQueryEntitiesAndIndex<T>(EGID entityGid, out uint index, out T[] array) where T : struct, IEntityStruct;
        bool TryQueryEntitiesAndIndex
            <T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group, out uint index, out T[] array)
            where T : struct, IEntityStruct;

        /// <summary>
        /// ECS is meant to work on a set of Entities. Working on a single entity is sometime necessary, but using
        /// the following functions inside a loop would be a mistake as performance can be significantly impacted
        /// return the buffer and the index of the entity inside the buffer using the input EGID
        /// </summary>
        /// <param name="entityGid"></param>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T[] QueryEntitiesAndIndex<T>(EGID entityGid, out uint index) where T : struct, IEntityStruct;
        T[] QueryEntitiesAndIndex<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group, out uint index)
            where T : struct, IEntityStruct;

        /// <summary>
        /// QueryUniqueEntity is a contract method that explicitly declare the intention to have just on entity in a
        /// specific group, usually used for GUI elements
        /// </summary>
        /// <param name="group"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ref T QueryUniqueEntity<T>(ExclusiveGroup.ExclusiveGroupStruct group) where T : struct, IEntityStruct;

        /// <summary>
        ///
        /// </summary>
        /// <param name="entityGid"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ref T QueryEntity<T>(EGID entityGid) where T : struct, IEntityStruct;
        ref T QueryEntity<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group) where T : struct, IEntityStruct;

        /// <summary>
        /// Fast and raw (therefore not safe) return of entities buffer
        /// Modifying a buffer would compromise the integrity of the whole DB
        /// so they are meant to be used only in performance critical path
        /// </summary>
        /// <param name="count"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T[] QueryEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out uint count)
            where T : struct, IEntityStruct;
        (T1[], T2[]) QueryEntities<T1, T2>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out uint count)
            where T1 : struct, IEntityStruct where T2 : struct, IEntityStruct;
        (T1[], T2[], T3[]) QueryEntities<T1, T2, T3>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out uint count)
            where T1 : struct, IEntityStruct where T2 : struct, IEntityStruct where T3 : struct, IEntityStruct;
        
        EntityCollection<T> QueryEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct)
            where T : struct, IEntityStruct;

        EntityCollections<T> QueryEntities<T>(ExclusiveGroup[] groups) where T : struct, IEntityStruct;
        EntityCollections<T1, T2> QueryEntities<T1, T2>(ExclusiveGroup[] groups)
            where T1 : struct, IEntityStruct where T2 : struct, IEntityStruct;

        /// <summary>
        /// this version returns a mapped version of the entity array so that is possible to find the
        /// index of the entity inside the returned buffer through it's EGID
        /// However mapping can be slow so it must be used for not performance critical paths
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="mapper"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        EGIDMapper<T> QueryMappedEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId)
            where T : struct, IEntityStruct;

        /// <summary>
        /// Execute an action on ALL the entities regardless the group. This function doesn't guarantee cache
        /// friendliness even if just EntityStructs are used.
        /// Safety checks are in place
        /// </summary>
        /// <param name="damageableGroups"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        void ExecuteOnAllEntities<T>(Action<T[], ExclusiveGroup.ExclusiveGroupStruct, uint, IEntitiesDB> action)
            where T : struct, IEntityStruct;

        void ExecuteOnAllEntities<T, W>(ref W value,
            Action<T[], ExclusiveGroup.ExclusiveGroupStruct, uint, IEntitiesDB, W> action)
            where T : struct, IEntityStruct;

        /// <summary>
        ///
        /// </summary>
        /// <param name="egid"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool Exists<T>(EGID egid) where T : struct, IEntityStruct;
        bool Exists(ExclusiveGroup.ExclusiveGroupStruct gid);

        /// <summary>
        ///
        /// </summary>
        /// <param name="group"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool HasAny<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct) where T : struct, IEntityStruct;

        /// <summary>
        ///
        /// </summary>
        /// <param name="groupStruct"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        uint Count<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct) where T : struct, IEntityStruct;

        /// <summary>
        ///
        /// </summary>
        /// <param name="egid"></param>
        /// <typeparam name="T"></typeparam>
        void PublishEntityChange<T>(EGID egid) where T : unmanaged, IEntityStruct;
    }
}