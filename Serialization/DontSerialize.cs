using Svelto.ECS.Internal;

namespace Svelto.ECS.Serialization
{
    public class DontSerialize<T> : IComponentSerializer<T> where T : unmanaged, _IInternalEntityComponent
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