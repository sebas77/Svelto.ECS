namespace Svelto.ECS.Serialization
{
    public static class SerializationEntityDescriptorTemplate<TType> where TType : ISerializableEntityDescriptor, new()
    {
        static SerializationEntityDescriptorTemplate()
        {
            var serializableEntityDescriptor = new TType();
            hash = serializableEntityDescriptor.hash;

            entityDescriptor = (ISerializableEntityDescriptor) EntityDescriptorTemplate<TType>.descriptor;
        }

        public static uint hash { get; }
        public static ISerializableEntityDescriptor entityDescriptor { get;  }
    }
}