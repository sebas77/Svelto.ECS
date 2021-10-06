namespace Svelto.ECS
{
    struct SerializableEntityComponent : IEntityComponent, INeedEGID
    {
        public uint descriptorHash;

        public EGID ID { get; set; }
    }
}