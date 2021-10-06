#if DEBUG && !PROFILE_SVELTO
#endif
namespace Svelto.ECS.Serialization
{
    public interface IComponentSerializer<T> where T : unmanaged, IEntityComponent
    {
        bool Serialize(in T value, ISerializationData serializationData);
        bool Deserialize(ref T value, ISerializationData serializationData);

        //Todo: We currently use the value 0 (which I am not even sure if it's not ambiguous) to mark an entity as dynamic size. If zero the systems assumes that the first word to deserialize
        //is the size of the entity. However this means that this field cannot reliably be used to know the size of the entity before hand, we need to either remove all the ambiguities in 
        //this sense or find another way to serialise dynamic size entities.
        uint size { get; }
    }
}