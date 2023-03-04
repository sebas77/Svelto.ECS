using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Attribute = System.Attribute;

namespace Svelto.ECS.Serialization
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PartialSerializerFieldAttribute : Attribute
    {}

    public class PartialSerializer<T> : IComponentSerializer<T>
        where T : unmanaged, _IInternalEntityComponent
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
                        var fieldType = myMembers[i].FieldType;
                        if (fieldType.ContainsCustomAttribute(typeof(DoNotSerializeAttribute)) &&
                            myMembers[i].IsPrivate == false)
                                throw new ECSException($"field cannot be serialised {fieldType} in {myType.FullName}");

                        var  offset = Marshal.OffsetOf<T>(myMembers[i].Name);
                        uint sizeOf;
                        if (fieldType == typeof(bool))
                            sizeOf = 1;
                        else
                            sizeOf = (uint)Marshal.SizeOf(fieldType);
                        
                        offsets.Add(((uint) offset.ToInt32(), sizeOf));
                        totalSize += sizeOf;
                    }
                }
            }

            if (myType.IsExplicitLayout == false)
                throw new ECSException($"PartialSerializer requires explicit layout {myType}");
#if SLOW_SVELTO_SUBMISSION            
            if (myType.GetProperties().Length > (ComponentBuilder<T>.HAS_EGID ? 1 : 0))
                throw new ECSException("serializable entity struct must be property less ".FastConcat(myType.FullName));
#endif                
            if (totalSize == 0)
                throw new ECSException($"{typeof(T).Name} is being serialized with {nameof(PartialSerializer<T>)} but has size 0!");
        }

        public bool Serialize(in T value, ISerializationData serializationData) 
        {
            unsafe
            {
                fixed (byte* dataptr = serializationData.data.ToArrayFast(out _))
                {
                    var entityComponent = value;
                    foreach ((uint offset, uint size) offset in offsets)
                    {
                        byte* srcPtr = (byte*) &entityComponent + offset.offset;
                        //todo move to Unsafe Copy when available as it is faster
                        Buffer.MemoryCopy(srcPtr, dataptr + serializationData.dataPos,
                            serializationData.data.count - serializationData.dataPos, offset.size);
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
                fixed (byte* dataptr = serializationData.data.ToArrayFast(out _))
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