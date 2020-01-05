namespace Svelto.ECS
{
    public struct EntityHierarchyStruct: IEntityStruct, INeedEGID
    {
        public readonly ExclusiveGroup.ExclusiveGroupStruct parentGroup;
            
        public EntityHierarchyStruct(ExclusiveGroup group): this() { parentGroup = group; }
            
        public EGID ID { get; set; }
    }
}