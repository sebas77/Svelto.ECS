#if DEBUG && !PROFILE_SVELTO
#endif

namespace Svelto.ECS.Serialization
{
    public interface IComponentSerializer<T> where T : unmanaged, IEntityComponent
    {
        bool Serialize(in T value, ISerializationData serializationData);
        bool Deserialize(ref T value, ISerializationData serializationData);

        uint size { get; }
    }
}