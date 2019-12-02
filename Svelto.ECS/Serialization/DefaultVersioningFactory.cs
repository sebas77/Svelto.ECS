using System.Collections.Generic;

namespace Svelto.ECS.Serialization
{
    public class DefaultVersioningFactory<T> : IDeserializationFactory where T : IEntityDescriptor, new()
    {
        public EntityStructInitializer BuildDeserializedEntity(EGID                          egid,
                                                               ISerializationData            serializationData,
                                                               ISerializableEntityDescriptor entityDescriptor,
                                                               SerializationType             serializationType,
                                                               IEntitySerialization          entitySerialization)
        {
            var initializer = _factory.BuildEntity<T>(egid, _implementors);
                
            entitySerialization.DeserializeEntityStructs(serializationData, entityDescriptor, ref initializer, SerializationType.Storage);

            return initializer;
        }
        
        public DefaultVersioningFactory(IEntityFactory factory) { _factory = factory; }
        public DefaultVersioningFactory(IEntityFactory factory, IEnumerable<object> implementors) { _factory = factory;
            _implementors = implementors;
        }
        
        readonly IEntityFactory _factory;
        readonly IEnumerable<object> _implementors;
    }
}