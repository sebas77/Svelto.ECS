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
            IComponentBuilder[] defaultEntities = EntityDescriptorTemplate<TType>.realDescriptor.componentsToBuild;

            var hashNameAttribute = Type.GetCustomAttribute<HashNameAttribute>();
            if (hashNameAttribute == null)
            {
                throw new Exception("HashName attribute not found on the serializable type ".FastConcat(Type.FullName));
            }

            Hash = DesignatedHash.Hash(Encoding.ASCII.GetBytes(hashNameAttribute._name));

            var (index, dynamicIndex) = SetupSpecialEntityComponent(defaultEntities, out ComponentsToBuild);
            if (index == -1)
            {
                index = ComponentsToBuild.Length - 1;
            }

            // Stores the hash of this EntityDescriptor
            ComponentsToBuild[index] = new ComponentBuilder<SerializableEntityComponent>(new SerializableEntityComponent
            {
                descriptorHash = Hash
            });

            // If the current serializable is an ExtendibleDescriptor, I have to update it.
            if (dynamicIndex != -1)
            {
                ComponentsToBuild[dynamicIndex] = new ComponentBuilder<EntityInfoComponent>(new EntityInfoComponent
                {
                    componentsToBuild = ComponentsToBuild
                });
            }

            /////
            var entitiesToSerialize = new FasterList<ISerializableComponentBuilder>();
            EntityComponentsToSerializeMap = new FasterDictionary<ComponentID, ISerializableComponentBuilder>();
            foreach (IComponentBuilder e in defaultEntities)
            {
                if (e is ISerializableComponentBuilder serializableEntityBuilder)
                {
                    EntityComponentsToSerializeMap[serializableEntityBuilder.getComponentID] = serializableEntityBuilder;
                    entitiesToSerialize.Add(serializableEntityBuilder);
                }
            }

            EntitiesToSerialize = entitiesToSerialize.ToArray();
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
                if (defaultEntities[i].GetEntityComponentType() == SerializableStructType)
                {
                    indexSerial = i;
                    --newLenght;
                }

                if (defaultEntities[i].GetEntityComponentType() == ComponentBuilderUtilities.ENTITY_INFO_COMPONENT)
                {
                    indexDynamic = i;
                }
            }

            componentsToBuild = new IComponentBuilder[newLenght];

            Array.Copy(defaultEntities, 0, componentsToBuild, 0, length);

            return (indexSerial, indexDynamic);
        }

        public IComponentBuilder[]             componentsToBuild   => ComponentsToBuild;
        public uint                            hash                => Hash;
        public Type                            realType            => Type;
        public ISerializableComponentBuilder[] componentsToSerialize => EntitiesToSerialize;

        static readonly IComponentBuilder[]                                             ComponentsToBuild;
        static readonly FasterDictionary<ComponentID, ISerializableComponentBuilder> EntityComponentsToSerializeMap;
        static readonly ISerializableComponentBuilder[]                                 EntitiesToSerialize;

        static readonly uint Hash;
        static readonly Type SerializableStructType = typeof(SerializableEntityComponent);
        static readonly Type Type                   = typeof(TType);
    }
}