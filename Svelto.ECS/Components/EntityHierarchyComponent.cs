namespace Svelto.ECS
{
    public struct EntityHierarchyComponent: IEntityComponent, INeedEGID
    {
        public readonly ExclusiveGroupStruct parentGroup;
            
        public EntityHierarchyComponent(ExclusiveGroup group): this() { parentGroup = group; }
            
        public EGID ID { get; set; }
    }
}