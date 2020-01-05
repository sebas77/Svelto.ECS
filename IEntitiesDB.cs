using System;

namespace Svelto.ECS
{
    public delegate void ExecuteOnAllEntitiesAction<T, W>(T[] entities, ExclusiveGroup.ExclusiveGroupStruct group,
        uint count, IEntitiesDB db, ref W value);

    public interface IEntitiesDB
    {
        ///////////////////////////////////////////////////
        // Query entities
        // ECS systems are meant to work on a set of Entities. These methods allow to iterate over entity
        // structs inside a given group or an array of groups
        ///////////////////////////////////////////////////

        /// <summary>
        /// Fast and raw return of entities buffer.
        /// </summary>
        /// <param name="groupStruct"></param>
        /// <param name="count"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T[] QueryEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out uint count)
            where T : struct, IEntityStruct;
        (T1[], T2[]) QueryEntities<T1, T2>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out uint count)
            where T1 : struct, IEntityStruct where T2 : struct, IEntityStruct;
        (T1[], T2[], T3[]) QueryEntities<T1, T2, T3>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out uint count)
            where T1 : struct, IEntityStruct where T2 : struct, IEntityStruct where T3 : struct, IEntityStruct;

        /// <summary>
        /// return entities that can be iterated through the EntityCollection iterator
        /// </summary>
        /// <param name="groupStruct"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        EntityCollection<T> QueryEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct)
            where T : struct, IEntityStruct;
        EntityCollection<T1, T2> QueryEntities<T1, T2>(ExclusiveGroup.ExclusiveGroupStruct groupStruct)
            where T1 : struct, IEntityStruct where T2 : struct, IEntityStruct;
        EntityCollection<T1, T2, T3> QueryEntities<T1, T2, T3>(ExclusiveGroup.ExclusiveGroupStruct groupStruct)
            where T1 : struct, IEntityStruct 
            where T2 : struct, IEntityStruct
            where T3 : struct, IEntityStruct;

        /// <summary>
        /// return entities found in multiple groups, that can be iterated through the EntityCollection iterator
        /// This method is useful to write abstracted engines
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        EntityCollections<T> QueryEntities<T>(ExclusiveGroup[] groups) where T : struct, IEntityStruct;
        EntityCollections<T1, T2> QueryEntities<T1, T2>(ExclusiveGroup[] groups)
            where T1 : struct, IEntityStruct where T2 : struct, IEntityStruct;

        ///////////////////////////////////////////////////
        // Query entities regardless the group
        // these methods are necessary to create abstracted engines. Engines that can iterate over entities regardless
        // the group
        ///////////////////////////////////////////////////

        /// <summary>
        /// Execute an action on ALL the entities regardless the group. This function doesn't guarantee cache
        /// friendliness even if just EntityStructs are used. Safety checks are in place,
        /// </summary>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        void ExecuteOnAllEntities<T>(Action<T[], ExclusiveGroup.ExclusiveGroupStruct, uint, IEntitiesDB> action)
            where T : struct, IEntityStruct;

        /// <summary>
        /// same as above, but can pass some external data to avoid allocations
        /// </summary>
        /// <param name="value"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="W"></typeparam>
        void ExecuteOnAllEntities<T, W>(ref W value, ExecuteOnAllEntitiesAction<T, W> action)
            where T : struct, IEntityStruct;

        void ExecuteOnAllEntities<T, W>(W value, Action<T[], ExclusiveGroup.ExclusiveGroupStruct, uint, IEntitiesDB, W> action)
            where T : struct, IEntityStruct;

        ///////////////////////////////////////////////////
        // Query single entities
        // ECS systems are meant to work on a set of Entities. Working on a single entity is sometime necessary, hence
        // the following methods
        // However Because of the double hashing required to identify a specific entity, these function are slower than
        // other query methods when used multiple times!
        ///////////////////////////////////////////////////

        /// <summary>
        /// QueryUniqueEntity is a contract method that explicitly declare the intention to have just on entity in a
        /// specific group, usually used for GUI elements
        /// </summary>
        /// <param name="group"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ref T QueryUniqueEntity<T>(ExclusiveGroup.ExclusiveGroupStruct group) where T : struct, IEntityStruct;

        /// <summary>
        /// return a specific entity by reference.
        /// </summary>
        /// <param name="entityGid"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ref T QueryEntity<T>(EGID entityGid) where T : struct, IEntityStruct;
        ref T QueryEntity<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group) where T : struct, IEntityStruct;

        /// <summary>
        ///
        ///QueryEntitiesAndIndex is useful to optimize cases when multiple entity structs from the same entity must
        /// be queried. This is the use case:
        ///
        ///ref var ghostPosition = ref entitiesDB.QueryEntitiesAndIndex<PositionEntityStruct>
        /// (MockupRenderingGroups.GhostCubeID, out var index)[index];
        ///ref var ghostScaling = ref entitiesDB.QueryEntities<ScalingEntityStruct>
        /// (MockupRenderingGroups.GhostCubeID.groupID, out _)[index];
        ///ref var ghostRotation = ref entitiesDB.QueryEntities<RotationEntityStruct>
        /// (MockupRenderingGroups.GhostCubeID.groupID, out _)[index];
        ///ref var ghostResource = ref entitiesDB.QueryEntities<GFXPrefabEntityStruct>
        /// (MockupRenderingGroups.GhostCubeID.groupID, out _)[index];
        ///
        /// </summary>
        /// <param name="entityGid"></param>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T[] QueryEntitiesAndIndex<T>(EGID entityGid, out uint index) where T : struct, IEntityStruct;
        T[] QueryEntitiesAndIndex<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group, out uint index)
            where T : struct, IEntityStruct;

        /// <summary>
        /// Like QueryEntitiesAndIndex and only way to get an index only if exists
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
        /// this method returns a mapped version of the entity array so that is possible to work on multiple entities
        /// inside the group through their EGID. This version skip a level of indirection so it's a bit faster than
        /// using QueryEntity multiple times (with different EGIDs).
        /// However mapping can be slow so it must be used for not performance critical paths
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        EGIDMapper<T> QueryMappedEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId)
            where T : struct, IEntityStruct;
        bool TryQueryMappedEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId, out EGIDMapper<T> mapper)
            where T : struct, IEntityStruct;

        ///////////////////////////////////////////////////
        // Utility methods
        ///////////////////////////////////////////////////

        /// <summary>
        /// check if a specific entity exists
        /// </summary>
        /// <param name="egid"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool Exists<T>(EGID egid) where T : struct, IEntityStruct;
        bool Exists<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group) where T : struct, IEntityStruct;
        bool Exists(ExclusiveGroup.ExclusiveGroupStruct gid);

        /// <summary>
        /// know if there is any entity struct in a specific group
        /// </summary>
        /// <param name="groupStruct"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool HasAny<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct) where T : struct, IEntityStruct;

        /// <summary>
        /// Count the number of entity structs in a specific group
        /// </summary>
        /// <param name="groupStruct"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        uint Count<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct) where T : struct, IEntityStruct;

        /// <summary>
        /// </summary>
        /// <param name="egid"></param>
        /// <typeparam name="T"></typeparam>
        void PublishEntityChange<T>(EGID egid) where T : unmanaged, IEntityStruct;
    }
}
