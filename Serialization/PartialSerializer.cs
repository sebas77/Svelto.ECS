using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Svelto.DataStructures;
using Attribute = System.Attribute;

namespace Svelto.ECS.Serialization
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PartialSerializerFieldAttribute : Attribute
    {}

    public class PartialSerializer<T> : ISerializer<T>
        where T : unmanaged, IEntityStruct
    {
        static PartialSerializer()
        {
            Type myType = typeof(T);
            FieldInfo[] myMembers = myType.GetFields();

            for (int i = 0; i < myMembers.Length; i++)
            {
                Object[] myAttributes = myMembers[i].GetCustomAttributes(true);
                for (int j = 0; j < myAttributes.Length; j++)
                {
                    if (myAttributes[j] is PartialSerializerFieldAttribute)
                    {
                        if (myMembers[i].FieldType == typeof(EGID))
                            throw new ECSException("EGID fields cannot be serialised ".FastConcat(myType.FullName));

                        var offset = Marshal.OffsetOf<T>(myMembers[i].Name);
                        var sizeOf = (uint)Marshal.SizeOf(myMembers[i].FieldType);
                        offsets.Add(((uint) offset.ToInt32(), sizeOf));
                        totalSize += sizeOf;
                    }
                }
            }

            if (myType.GetProperties().Length > (EntityBuilder<T>.HAS_EGID ? 1 : 0))
                throw new ECSException("serializable entity struct must be property less ".FastConcat(myType.FullName));
        }

        public bool Serialize(in T value, ISerializationData serializationData)
        {
            unsafe
            {
                fixed (byte* dataptr = serializationData.data.ToArrayFast())
                {
                    var entityStruct = value;
                    foreach ((uint offset, uint size) offset in offsets)
                    {
                        byte* srcPtr = (byte*) &entityStruct + offset.offset;
                        //todo move to Unsafe Copy when available as it is faster
                        Buffer.MemoryCopy(srcPtr, dataptr + serializationData.dataPos,
                            serializationData.data.Count - serializationData.dataPos, offset.size);
                        serializationData.dataPos += offset.size;
                    }
                }
            }

            return true;
        }

        public bool Deserialize(ref T value, ISerializationData serializationData)
        {
            unsafe
            {
                T tempValue = value; //todo: temporary solution I want to get rid of this copy
                fixed (byte* dataptr = serializationData.data.ToArrayFast())
                    foreach ((uint offset, uint size) offset in offsets)
                    {
                        byte* dstPtr = (byte*) &tempValue + offset.offset;
                        //todo move to Unsafe Copy when available as it is faster
                        Buffer.MemoryCopy(dataptr + serializationData.dataPos, dstPtr, offset.size, offset.size);
                        serializationData.dataPos += offset.size;
                    }

                value = tempValue; //todo: temporary solution I want to get rid of this copy
            }

            return true;
        }

        public uint size => totalSize;

        static readonly FasterList<(uint, uint)> offsets = new FasterList<(uint, uint)>();
        static readonly uint totalSize;
    }
}