namespace Svelto.ECS
{
    public delegate void ExecuteOnAllEntitiesAction<T, W>(T[] prefabStruct, ExclusiveGroupStruct group,
        uint count, EntitiesDB db, ref W instances);
    public delegate void ExecuteOnAllEntitiesAction<T>(T[] entities, ExclusiveGroupStruct group,
                                                       uint count, EntitiesDB db);
}
