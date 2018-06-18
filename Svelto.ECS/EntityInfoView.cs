namespace Svelto.ECS
{
    public struct EntityInfoView : IEntityStruct
    {
        public EGID ID { get; set; }
        
        public IEntityBuilder[] entityToBuild;
    }
}