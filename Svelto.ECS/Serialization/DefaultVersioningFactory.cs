using System.Collections.Generic;

namespace Svelto.ECS.Serialization
{
    public class DefaultVersioningFactory<T> : IDeserializationFactory where T : IEntityDescriptor, new()
    {
        readonly IEntityFactory      _factory;
        readonly IEnumerable<object> _implementors;

        public DefaultVersioningFactory(IEntityFactory factory)
        {
            _factory = factory;
        }

        public DefaultVersioningFactory(IEntityFactory factory, IEnumerable<object> implementors)
        {
            _factory = factory;
            _implementors = implementors;
        }

        public EntityComponentInitializer BuildDeserializedEntity(EGID egid,
            ISerializationData serializationData,
            ISerializableEntityDescriptor entityDescriptor,
            int serializationType,
            IEntitySerialization entitySerialization)
        {
            var initializer = _factory.BuildEntity<T>(egid, _implementors);

            entitySerialization.DeserializeEntityComponents(serializationData, entityDescriptor, ref initializer,
                serializationType);

            return initializer;
        }
    }
}