using Svelto.Common;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Serialization
{
    public class DefaultSerializer<T> : IComponentSerializer<T> where T : unmanaged, _IInternalEntityComponent
    {
        static DefaultSerializer()
        {
            var _type = typeof(T);

            foreach (var field in _type.GetFields())
            {
                var fieldFieldType = field.FieldType;
                if (fieldFieldType.ContainsCustomAttribute(typeof(DoNotSerializeAttribute)) &&
                    field.IsPrivate == false)
                    throw new ECSException($"field cannot be serialised {fieldFieldType} in {_type.FullName}");
            }
#if SLOW_SVELTO_SUBMISSION            
            if (_type.GetProperties().Length > (ComponentBuilder<T>.HAS_EGID ? 1 : 0))
                throw new ECSException("serializable entity struct must be property less ".FastConcat(_type.FullName));
#endif
        }

        public uint size => (uint)MemoryUtilities.SizeOf<T>();

        public bool Serialize(in T value, ISerializationData serializationData)
        {
            DefaultSerializerUtils.CopyToByteArray(value, serializationData.data.ToArrayFast(out _),
                serializationData.dataPos);

            serializationData.dataPos += (uint)size;

            return true;
        }

        public bool Deserialize(ref T value, ISerializationData serializationData)
        {
            value = DefaultSerializerUtils.CopyFromByteArray<T>(serializationData.data.ToArrayFast(out _),
                serializationData.dataPos);

            serializationData.dataPos += (uint)size;

            return true;
        }
    }
}