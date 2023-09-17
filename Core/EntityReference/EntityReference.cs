using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable 660,661

namespace Svelto.ECS
{
    /// <summary>
    /// Todo: EntityReference shouldn't map EGIDs as dictionaries keys but directly the indices in the EntityDB arrays
    /// </summary>
    [Serialization.DoNotSerialize] //it's not a serializable field for svelto serializable system
    [Serializable] 
    [StructLayout(LayoutKind.Explicit)]
    public struct EntityReference : IEquatable<EntityReference>
    {
        [FieldOffset(0)] public readonly uint uniqueID;
        [FieldOffset(4)] public readonly uint version;
        [FieldOffset(0)] readonly ulong _GID;

        internal uint index => uniqueID - 1;

        public static bool operator ==(EntityReference obj1, EntityReference obj2)
        {
            return obj1._GID == obj2._GID;
        }

        public static bool operator !=(EntityReference obj1, EntityReference obj2)
        {
            return obj1._GID != obj2._GID;
        }

        public override int GetHashCode() { return _GID.GetHashCode(); }

        public EntityReference(uint uniqueId) : this(uniqueId, 0) {}

        public EntityReference(ulong GID):this() { _GID = GID; }

        public EntityReference(uint uniqueId, uint version) : this()
        {
            _GID = MAKE_GLOBAL_ID(uniqueId, version);
        }

        public bool Equals(EntityReference other)
        {
            return _GID == other._GID;
        }

        public bool Equals(EntityReference x, EntityReference y)
        {
            return x._GID == y._GID;
        }

        public override string ToString()
        {
            return "id:".FastConcat(uniqueID).FastConcat(" version:").FastConcat(version);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EGID ToEGID(EntitiesDB entitiesDB)
        {
            DBC.ECS.Check.Require(this != Invalid, "Invalid Reference Used");

            return entitiesDB.GetEGID(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ToEGID(EntitiesDB entitiesDB, out EGID egid)
        {
            return entitiesDB.TryGetEGID(this, out egid);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(EntitiesDB entitiesDB)
        {
            return this != Invalid && entitiesDB.TryGetEGID(this, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ToULong()
        {
            return _GID;
        }

        static ulong MAKE_GLOBAL_ID(uint uniqueId, uint version)
        {
            return (ulong)version << 32 | ((ulong)uniqueId & 0xFFFFFFFF);
        }

        public static EntityReference Invalid => default;
    }
}
