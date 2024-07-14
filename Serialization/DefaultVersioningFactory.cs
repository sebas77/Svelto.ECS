using Svelto.Common;

namespace Svelto.ECS.Serialization
{
    //TODO: Unit test. Delete this comment once Unit test is written
#if ENABLE_IL2CPP
    [UnityEngine.Scripting.Preserve]
#endif    
    public class DefaultVersioningFactory<T> : IDeserializationFactory where T : ISerializableEntityDescriptor, new()
    {
        public EntityInitializer BuildDeserializedEntity(EGID egid, ISerializationData serializationData,
            ISerializableEntityDescriptor entityDescriptor, int serializationType,
            IEntitySerialization entitySerialization, IEntityFactory factory, bool enginesRootIsDeserializationOnly)
        {
            var newEntityDescriptor = EntityDescriptorTemplate<T>.realDescriptor as  ISerializableEntityDescriptor;
                    
            var entityDescriptorEntitiesToSerialize = enginesRootIsDeserializationOnly
                ? newEntityDescriptor.componentsToSerialize
                : newEntityDescriptor.componentsToBuild;

            var initializer = (factory as IEntitySerializationFactory).BuildEntity(egid, entityDescriptorEntitiesToSerialize, TypeCache<T>.type);

            entitySerialization.DeserializeEntityComponents(serializationData, entityDescriptor, ref initializer,
                serializationType);

            return initializer;
        }
    }
}