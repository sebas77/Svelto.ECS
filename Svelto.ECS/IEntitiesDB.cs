namespace Svelto.ECS
{
    public interface IEntitiesDB: IObsoleteInterfaceDb
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
        bool TryQueryEntitiesAndIndex<T>(int  id, int group, out uint index, out T[] array) where T : IEntityStruct;
        bool TryQueryEntitiesAndIndex<T>(EGID entityGid, out uint index, out T[]  array) where T : IEntityStruct;
        bool TryQueryEntitiesAndIndex<T>(int id, ExclusiveGroup.ExclusiveGroupStruct group, out uint index, out T[] array) where T : IEntityStruct;
        
        ref T QueryUniqueEntity<T>(ExclusiveGroup.ExclusiveGroupStruct group) where T : IEntityStruct;
        ref T QueryUniqueEntity<T>(int group) where T : IEntityStruct;
        
        ref T  QueryEntity<T>(EGID    entityGid) where T : IEntityStruct;
        ref T  QueryEntity<T>(int id, ExclusiveGroup.ExclusiveGroupStruct group) where T : IEntityStruct;
        ref T  QueryEntity<T>(int id, int group) where T : IEntityStruct;
        /// <summary>
        /// Fast and raw (therefore not safe) return of entities buffer
        /// Modifying a buffer would compromise the integrity of the whole DB
        /// so they are meant to be used only in performance critical path
        /// </summary>
        /// <param name="count"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T[] QueryEntities<T>(int group, out int count) where T : IEntityStruct;

        T[] QueryEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out int count) where T : IEntityStruct;
        (T1[], T2[]) QueryEntities<T1, T2>(int group, out int count) where T1 : IEntityStruct where T2 : IEntityStruct;

        (T1[], T2[]) QueryEntities<T1, T2>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out int count) 
            where T1 : IEntityStruct where T2 : IEntityStruct;
        /// <summary>
        /// this version returns a mapped version of the entity array so that is possible to find the
        /// index of the entity inside the returned buffer through it's EGID
        /// However mapping can be slow so it must be used for not performance critical paths
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="mapper"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        EGIDMapper<T> QueryMappedEntities<T>(int groupID) where T : IEntityStruct;
        EGIDMapper<T> QueryMappedEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId) where T : IEntityStruct;
        /// <summary>
        /// Execute an action on entities. Be sure that the action is not capturing variables
        /// otherwise you will allocate memory which will have a great impact on the execution performance.
        /// ExecuteOnEntities can be used to iterate safely over entities, several checks are in place
        /// to be sure that everything will be done correctly.
        /// Cache friendliness is guaranteed if only Entity Structs are used, but 
        /// </summary>
        /// <param name="egid"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        void ExecuteOnEntities<T>(int groupID, EntitiesAction<T> action) where T : IEntityStruct;
        void ExecuteOnEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId, EntitiesAction<T> action) where T : IEntityStruct;
        void ExecuteOnEntities<T, W>(int groupID, ref W value, EntitiesAction<T, W> action) where T : IEntityStruct;
        void ExecuteOnEntities<T, W>(ExclusiveGroup.ExclusiveGroupStruct groupStructId, ref W value, EntitiesAction<T, W> action) where T : IEntityStruct;
        /// <summary>
        /// Execute an action on ALL the entities regardless the group. This function doesn't guarantee cache
        /// friendliness even if just EntityStructs are used. 
        /// Safety checks are in place 
        /// </summary>
        /// <param name="damageableGroups"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        void ExecuteOnAllEntities<T>(AllEntitiesAction<T> action) where T : IEntityStruct;
        void ExecuteOnAllEntities<T, W>(ref W  value, AllEntitiesAction<T, W> action) where T : IEntityStruct;
        void ExecuteOnAllEntities<T>(ExclusiveGroup[] groups, EntitiesAction<T> action) where T : IEntityStruct;
        void ExecuteOnAllEntities<T, W>(ExclusiveGroup[] groups, ref W value, EntitiesAction<T, W> action) where T : IEntityStruct;
        /// <summary>
        /// ECS is meant to work on a set of Entities. Working on a single entity is sometime necessary, but using
        /// the following functions inside a loop would be a mistake as performance can be significantly impacted
        /// Execute an action on a specific Entity. Be sure that the action is not capturing variables
        /// otherwise you will allocate memory which will have a great impact on the execution performance
        /// </summary>
        /// <param name="egid"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        void ExecuteOnEntity<T>(EGID egid, EntityAction<T> action) where T : IEntityStruct;
        void ExecuteOnEntity<T>(int id, int groupid, EntityAction<T> action) where T : IEntityStruct;
        void ExecuteOnEntity<T>(int id,  ExclusiveGroup.ExclusiveGroupStruct groupid, EntityAction<T> action) where T : IEntityStruct;
        void ExecuteOnEntity<T, W>(EGID egid, ref W value, EntityAction<T, W> action) where T : IEntityStruct;
        void ExecuteOnEntity<T, W>(int id,  int groupid, ref W value, EntityAction<T, W> action) where T : IEntityStruct;
        void ExecuteOnEntity<T, W>(int id,  ExclusiveGroup.ExclusiveGroupStruct groupid, ref W value, EntityAction<T, W> action) where T : IEntityStruct;

        bool Exists<T>(EGID egid) where T : IEntityStruct;
        bool Exists<T>(int id, int groupid) where T : IEntityStruct;
        bool Exists (ExclusiveGroup.ExclusiveGroupStruct gid);
        
        bool HasAny<T>(int group) where T:IEntityStruct;
        bool HasAny<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct) where T:IEntityStruct;
        
        int Count<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct)  where T:IEntityStruct;
        int Count<T>(int groupStruct)  where T:IEntityStruct;
    }

    public delegate void EntityAction<T, W>(ref T target, ref W       value);
    public delegate void EntityAction<T>(ref    T target);

    public delegate void AllEntitiesAction<T, W>(ref T target, ref W value, IEntitiesDB entitiesDb);
    public delegate void AllEntitiesAction<T>(ref T target, IEntitiesDB entitiesDb);
    
    public delegate void EntitiesAction<T, W>(ref T target, ref W value, EntityActionData extraParams);
    public delegate void EntitiesAction<T>(ref T target, EntityActionData extraParams);

    public struct EntityActionData
    {
        public readonly IEntitiesDB entitiesDB;
        public readonly int entityIndex;

        public EntityActionData(IEntitiesDB entitiesDb, int index)
        {
            this.entitiesDB = entitiesDb;
            entityIndex = index;
        }
    }
}