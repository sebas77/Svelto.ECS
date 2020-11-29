using System;
using System.Runtime.InteropServices;

#pragma warning disable 660,661

namespace Svelto.ECS
{
    [Serialization.DoNotSerialize]
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct EntityReference : IEquatable<EntityReference>
    {
        [FieldOffset(0)] public readonly uint uniqueID;
        [FieldOffset(4)] public readonly uint version;
        [FieldOffset(0)] readonly ulong _GID;

        public static bool operator ==(EntityReference obj1, EntityReference obj2)
        {
            return obj1._GID == obj2._GID;
        }

        public static bool operator !=(EntityReference obj1, EntityReference obj2)
        {
            return obj1._GID != obj2._GID;
        }

        public EntityReference(uint uniqueId) : this(uniqueId, 0) {}

        public EntityReference(uint uniqueId, uint version) : this()
        {
            _GID = MAKE_GLOBAL_ID(uniqueId, version);
        }

        static ulong MAKE_GLOBAL_ID(uint uniqueId, uint version)
        {
            return (ulong)version << 32 | ((ulong)uniqueId & 0xFFFFFFFF);
        }

        public static EntityReference Invalid => new EntityReference(uint.MaxValue, uint.MaxValue);

        public bool Equals(EntityReference other)
        {
            return _GID == other._GID;
        }

        public bool Equals(EntityReference x, EntityReference y)
        {
            return x == y;
        }

        public override string ToString()
        {
            return "id:".FastConcat(uniqueID).FastConcat(" version:").FastConcat(version);
        }
    }
}
