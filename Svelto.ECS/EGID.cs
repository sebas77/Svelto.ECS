using System;
using System.Collections.Generic;
#pragma warning disable 660,661

namespace Svelto.ECS
{
    public struct EGID:IEquatable<EGID>,IEqualityComparer<EGID>,IComparable<EGID>
    {
        readonly long _GID;
        
        public int entityID
        {
            get { return (int) (_GID & 0xFFFFFFFF); }
        }
        
        public ExclusiveGroup.ExclusiveGroupStruct groupID
        {
            get { return new ExclusiveGroup.ExclusiveGroupStruct((int) (_GID >> 32)); }
        }

        public static bool operator ==(EGID obj1, EGID obj2)
        {
            return obj1._GID == obj2._GID;
        }
        
        public static bool operator !=(EGID obj1, EGID obj2)
        {
            return obj1._GID != obj2._GID;
        }
        
        public EGID(int entityID, ExclusiveGroup.ExclusiveGroupStruct groupID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, groupID);
        }
        
        static long MAKE_GLOBAL_ID(int entityId, int groupId)
        {
            return (long)groupId << 32 | ((long)(uint)entityId & 0xFFFFFFFF);
        }

        public static explicit operator int(EGID id)
        {
            return id.entityID;
        }
        
        public static explicit operator long(EGID id)
        {
            return id._GID;
        }

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
        
        internal EGID(int entityID, int groupID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, groupID);
        }
    }
}