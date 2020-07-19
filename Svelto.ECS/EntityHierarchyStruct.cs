namespace Svelto.ECS
{
    public struct EntityHierarchyStruct: IEntityComponent, INeedEGID
    {
        public readonly ExclusiveGroupStruct parentGroup;
            
        public EntityHierarchyStruct(ExclusiveGroup @group): this() { parentGroup = group; }
            
        public EGID ID { get; set; }
    }
}