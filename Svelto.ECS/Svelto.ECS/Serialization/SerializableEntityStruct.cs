namespace Svelto.ECS
{
    internal struct SerializableEntityStruct : IEntityStruct, INeedEGID
    {
        public uint descriptorHash;

        public EGID ID { get; set; }
    }
}