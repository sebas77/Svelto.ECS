using System;
using System.Collections.Generic;

#pragma warning disable 660,661

namespace Svelto.ECS
{
    //todo: add debug map
    [Serialization.DoNotSerialize]
    [Serializable]
    public struct EGID:IEquatable<EGID>,IEqualityComparer<EGID>,IComparable<EGID>
    {
        public uint entityID => (uint) (_GID & 0xFFFFFFFF);

        public ExclusiveGroup.ExclusiveGroupStruct groupID => new ExclusiveGroup.ExclusiveGroupStruct((uint) (_GID >> 32));

        public static bool operator ==(EGID obj1, EGID obj2)
        {
            return obj1._GID == obj2._GID;
        }

        public static bool operator !=(EGID obj1, EGID obj2)
        {
            return obj1._GID != obj2._GID;
        }

        public EGID(uint entityID, ExclusiveGroup.ExclusiveGroupStruct groupID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, groupID);
        }

        static ulong MAKE_GLOBAL_ID(uint entityId, uint groupId)
        {
            return (ulong)groupId << 32 | ((ulong)entityId & 0xFFFFFFFF);
        }

        public static explicit operator uint(EGID id)
        {
            return id.entityID;
        }

        //in the way it's used, ulong must be always the same for each id/group
        public static explicit operator ulong(EGID id) { return id._GID; }

        public bool Equals(EGID other)
        {
            return _GID == other._GID;
        }

        public bool Equals(EGID x, EGID y)
        {
            return x == y;
        }

        public int GetHashCode(EGID obj)
        {
            return _GID.GetHashCode();
        }

        public int CompareTo(EGID other)
        {
            return _GID.CompareTo(other._GID);
        }
        
        internal EGID(uint entityID, uint groupID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, groupID);
        }

        public override string ToString()
        {
            return "id ".FastConcat(entityID).FastConcat(" group ").FastConcat(groupID);
        }

        readonly ulong _GID;
    }
}
