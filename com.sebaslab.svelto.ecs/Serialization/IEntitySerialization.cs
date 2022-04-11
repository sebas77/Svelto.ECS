using System.Runtime.CompilerServices;

namespace Svelto.ECS.Serialization
{
    public interface IEntitySerialization
    {
        /// <summary>
        /// Fill the serializationData of the entitiesToSerialize of this descriptor
        /// </summary>
        /// <param name="egid"></param>
        /// <param name="serializedData"></param>
        /// <param name="serializationType"></param>
        /// <returns>Size in bytes of the newly instantiated entity</returns>
        void SerializeEntity(EGID egid, ISerializationData serializationData, int serializationType);

        /// <summary>
        /// Deserialize a serializationData and copy directly onto the appropriate entities
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataPos"></param>
        /// <param name="serializationType"></param>
        void DeserializeEntity(ISerializationData serializationData, int serializationType);

        /// <summary>
        /// Deserialize a serializationData and copy directly onto the appropriate entities with explicit EGID
        /// </summary>
        /// <param name="egid"></param>
        /// <param name="data"></param>
        /// <param name="dataPos"></param>
        /// <param name="serializationType"></param>
        void DeserializeEntity(EGID egid, ISerializationData serializationData, int serializationType);
        
        /// <summary>
        /// Deserialize a serializationData and copy directly to an previously created EntityInitializer
        /// </summary>
        /// <param name="serializationData"></param>
        /// <param name="entityDescriptor"></param>
        /// <param name="initializer"></param>
        /// <param name="serializationType"></param>
        void DeserializeEntityComponents(ISerializationData serializationData,
            ISerializableEntityDescriptor entityDescriptor,
            ref EntityInitializer initializer, int serializationType);

        /// <summary>
        /// Contrary to the other Deserialize methods that assume that the entity exists, this method is used to deserialise
        /// a new Entity
        /// </summary>
        /// <param name="egid"></param>
        /// <param name="serializationData"></param>
        /// <param name="serializationType"></param>
        /// <returns></returns>
        EntityInitializer DeserializeNewEntity(EGID egid, ISerializationData serializationData,
                                                        int serializationType);
        /// <summary>
        /// Skips over entities without deserializing them, but incrementing the data position of the serialization data
        /// as if it had 
        /// </summary>
        /// <param name="serializationData"></param>
        /// <param name="serializationType"></param>
        /// <param name="numberOfEntities"></param>
        void SkipEntityDeserialization(ISerializationData serializationData, int serializationType,
            int numberOfEntities);

        /// <summary>
        /// Special Entity Swap method that works without knowing the EntityDescriptor to swap
        /// </summary>
        /// <param name="fromEGID"></param>
        /// <param name="toEGID"></param>
        /// <param name="caller"></param>
        void DeserializeEntityToSwap(EGID fromEGID, EGID toEGID,  [CallerMemberName] string caller = null);

        /// <summary>
        /// Special Entity delete method that works without knowing the EntityDescriptor to delete
        /// </summary>
        /// <param name="egid"></param>
        void DeserializeEntityToDelete(EGID egid, [CallerMemberName] string caller = null);

        uint GetHashFromGroup(ExclusiveGroupStruct groupStruct);

        ExclusiveGroupStruct GetGroupFromHash(uint groupHash);
        
        void RegisterSerializationFactory<T>(IDeserializationFactory deserializationFactory)
            where T : ISerializableEntityDescriptor, new();

        T DeserializeEntityComponent<T>(ISerializationData serializationData,
            ISerializableEntityDescriptor entityDescriptor, int serializationType) 
            where T : unmanaged, IEntityComponent;
    }
}