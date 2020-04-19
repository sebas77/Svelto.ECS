using System;
using System.Reflection;
using System.Text;
using Svelto.DataStructures;
using Svelto.Utilities;

namespace Svelto.ECS.Serialization
{
    public static class DesignatedHash
    {
        public static readonly Func<byte[], uint> Hash = Murmur3.MurmurHash3_x86_32;
    }

    public abstract class SerializableEntityDescriptor<TType> : ISerializableEntityDescriptor
        where TType : IEntityDescriptor, new()
    {
        static SerializableEntityDescriptor()
        {
            IComponentBuilder[] defaultEntities = EntityDescriptorTemplate<TType>.descriptor.componentsToBuild;

            var hashNameAttribute = _type.GetCustomAttribute<HashNameAttribute>();
            if (hashNameAttribute == null)
            {
                throw new Exception(
                    "HashName attribute not found on the serializable type ".FastConcat(_type.FullName));
            }

            _hash = DesignatedHash.Hash(Encoding.ASCII.GetBytes(hashNameAttribute._name));

            var (index, dynamicIndex) = SetupSpecialEntityComponent(defaultEntities, out ComponentsToBuild);
            if (index == -1)
            {
                index = ComponentsToBuild.Length - 1;
            }

            // Stores the hash of this EntityDescriptor
            ComponentsToBuild[index] = new ComponentBuilder<SerializableEntityComponent>(new SerializableEntityComponent
            {
                descriptorHash = _hash
            });

            // If the current serializable is an ExtendibleDescriptor, I have to update it.
            if (dynamicIndex != -1)
            {
                ComponentsToBuild[dynamicIndex] = new ComponentBuilder<EntityInfoComponentView>(new EntityInfoComponentView
                {
                    componentsToBuild = ComponentsToBuild
                });
            }

            /////
            var entitiesToSerialize = new FasterList<ISerializableComponentBuilder>();
            _entityComponentsToSerializeMap = new FasterDictionary<RefWrapper<Type>, ISerializableComponentBuilder>();
            foreach (IComponentBuilder e in defaultEntities)
            {
                if (e is ISerializableComponentBuilder serializableEntityBuilder)
                {
                    var entityType = serializableEntityBuilder.GetEntityComponentType();
                    _entityComponentsToSerializeMap[new RefWrapper<Type>(entityType)] = serializableEntityBuilder;
                    entitiesToSerialize.Add(serializableEntityBuilder);
                }
            }

            _entitiesToSerialize = entitiesToSerialize.ToArray();
        }

        static (int indexSerial, int indexDynamic) SetupSpecialEntityComponent
            (IComponentBuilder[] defaultEntities, out IComponentBuilder[] componentsToBuild)
        {
            int length    = defaultEntities.Length;
            int newLenght = length + 1;

            int indexSerial  = -1;
            int indexDynamic = -1;

            for (var i = 0; i < length; ++i)
            {
                if (defaultEntities[i].GetEntityComponentType() == _serializableStructType)
                {
                    indexSerial = i;
                    --newLenght;
                }

                if (defaultEntities[i].GetEntityComponentType() == EntityBuilderUtilities.ENTITY_STRUCT_INFO_VIEW)
                {
                    indexDynamic = i;
                }
            }

            componentsToBuild = new IComponentBuilder[newLenght];

            Array.Copy(defaultEntities, 0, componentsToBuild, 0, length);

            return (indexSerial, indexDynamic);
        }

        public IComponentBuilder[]             componentsToBuild => ComponentsToBuild;
        public uint                         hash                    => _hash;
        public ISerializableComponentBuilder[] entitiesToSerialize     => _entitiesToSerialize;

        static readonly IComponentBuilder[]                                               ComponentsToBuild;
        static readonly FasterDictionary<RefWrapper<Type>, ISerializableComponentBuilder> _entityComponentsToSerializeMap;
        static readonly ISerializableComponentBuilder[]                                   _entitiesToSerialize;

        static readonly uint _hash;
        static readonly Type _serializableStructType = typeof(SerializableEntityComponent);
        static readonly Type _type                   = typeof(TType);
    }
}