using Svelto.DataStructures;

namespace Svelto.ECS
{
    public delegate void ExecuteOnAllEntitiesAction<T, W>(IBuffer<T> prefabStruct, ExclusiveGroupStruct group,
        uint count, EntitiesDB db, ref W instances);
    public delegate void ExecuteOnAllEntitiesAction<T>(IBuffer<T> entities, ExclusiveGroupStruct group,
                                                       uint count, EntitiesDB db);
}
