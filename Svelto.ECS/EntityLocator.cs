using System;
using System.Runtime.InteropServices;

#pragma warning disable 660,661

namespace Svelto.ECS
{
    [Serialization.DoNotSerialize]
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct EntityLocator : IEquatable<EntityLocator>
    {
        [FieldOffset(0)] public readonly uint uniqueID;
        [FieldOffset(4)] public readonly uint version;
        [FieldOffset(0)] readonly ulong _GID;

        public static bool operator ==(EntityLocator obj1, EntityLocator obj2)
        {
            return obj1._GID == obj2._GID;
        }

        public static bool operator !=(EntityLocator obj1, EntityLocator obj2)
        {
            return obj1._GID != obj2._GID;
        }

        public EntityLocator(uint uniqueId) : this(uniqueId, 0) {}

        public EntityLocator(uint uniqueId, uint version) : this()
        {
            _GID = MAKE_GLOBAL_ID(uniqueId, version);
        }

        static ulong MAKE_GLOBAL_ID(uint uniqueId, uint version)
        {
            return (ulong)version << 32 | ((ulong)uniqueId & 0xFFFFFFFF);
        }

        public static EntityLocator Invalid => new EntityLocator(uint.MaxValue, uint.MaxValue);

        public bool Equals(EntityLocator other)
        {
            return _GID == other._GID;
        }

        public bool Equals(EntityLocator x, EntityLocator y)
        {
            return x == y;
        }

        public override string ToString()
        {
            return "id:".FastConcat(uniqueID).FastConcat(" version:").FastConcat(version);
        }
    }
}
