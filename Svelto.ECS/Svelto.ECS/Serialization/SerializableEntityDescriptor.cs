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
            IEntityBuilder[] defaultEntities = EntityDescriptorTemplate<TType>.descriptor.entitiesToBuild;

            var hashNameAttribute = _type.GetCustomAttribute<HashNameAttribute>();
            if (hashNameAttribute == null)
            {
                throw new Exception("HashName attribute not found on the serializable type ".FastConcat(_type.FullName));
            }

            _hash = DesignatedHash.Hash(Encoding.ASCII.GetBytes(hashNameAttribute._name));

            var (index, dynamicIndex) = SetupSpecialEntityStruct(defaultEntities, out _entitiesToBuild);
            if (index == -1)
            {
                index = _entitiesToBuild.Length - 1;
            }

            // Stores the hash of this EntityDescriptor
            _entitiesToBuild[index] = new EntityBuilder<SerializableEntityStruct>
            (
                new SerializableEntityStruct
                {
                    descriptorHash = _hash
                }
            );

            // If the current serializable is an ExtendibleDescriptor, I have to update it.
            if (dynamicIndex != -1)
            {
                _entitiesToBuild[dynamicIndex] = new EntityBuilder<EntityStructInfoView>
                (
                    new EntityStructInfoView
                    {
                        entitiesToBuild = _entitiesToBuild
                    }
                );
            }

            /////
            var entitiesToSerialize = new FasterList<ISerializableEntityBuilder>();
            _entitiesToSerializeMap = new FasterDictionary<RefWrapper<Type>, ISerializableEntityBuilder>();
            foreach (IEntityBuilder e in defaultEntities)
            {
                if (e is ISerializableEntityBuilder serializableEntityBuilder)
                {
                    var entityType = serializableEntityBuilder.GetEntityType();
                    _entitiesToSerializeMap[new RefWrapper<Type>(entityType)] = serializableEntityBuilder;
                    entitiesToSerialize.Add(serializableEntityBuilder);
                }
            }
            
            _entitiesToSerialize = entitiesToSerialize.ToArray();
        }

        static (int indexSerial, int indexDynamic) SetupSpecialEntityStruct(IEntityBuilder[] defaultEntities,
            out IEntityBuilder[] entitiesToBuild)
        {
            int length = defaultEntities.Length;
            int newLenght = length + 1;

            int indexSerial = -1;
            int indexDynamic = -1;

            for (var i = 0; i < length; ++i)
            {
                if (defaultEntities[i].GetEntityType() == _serializableStructType)
                {
                    indexSerial = i;
                    --newLenght;
                }

                if (defaultEntities[i].GetEntityType() == EntityBuilderUtilities.ENTITY_STRUCT_INFO_VIEW)
                {
                    indexDynamic = i;
                }
            }

            entitiesToBuild = new IEntityBuilder[newLenght];

            Array.Copy(defaultEntities, 0, entitiesToBuild, 0, length);

            return (indexSerial, indexDynamic);
        }
        
        public void CopySerializedEntityStructs(in EntityStructInitializer sourceInitializer, in EntityStructInitializer destinationInitializer, SerializationType serializationType)
        {
            foreach (ISerializableEntityBuilder e in entitiesToSerialize)
            {
                e.CopySerializedEntityStructs(sourceInitializer, destinationInitializer, serializationType);
            }
        }

        public IEntityBuilder[]             entitiesToBuild     => _entitiesToBuild;
        public uint                         hash                => _hash;
        public ISerializableEntityBuilder[] entitiesToSerialize => _entitiesToSerialize;

        static readonly IEntityBuilder[]                                               _entitiesToBuild;
        static readonly FasterDictionary<RefWrapper<Type>, ISerializableEntityBuilder> _entitiesToSerializeMap;
        static readonly ISerializableEntityBuilder[]                                   _entitiesToSerialize;

        static readonly uint _hash;
        static readonly Type _serializableStructType = typeof(SerializableEntityStruct);
        static readonly Type _type                   = typeof(TType);
    }
}
