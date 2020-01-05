namespace Svelto.ECS.Serialization
{
    public class DontSerialize<T> : ISerializer<T> where T : unmanaged, IEntityStruct
    {
        public uint size => 0;

        public bool Serialize(in T value, ISerializationData serializationData)
        {
            return false;
        }

        public bool Deserialize(ref T value, ISerializationData serializationData)
        {
            return false;
        }
    }
}