using System;
using System.Collections.Generic;

namespace Svelto.ECS
{
    public struct EGID:IEquatable<long>,IEqualityComparer<long>
    {
        long _GID;
        
        public int entityID
        {
            get { return (int) (_GID & 0xFFFFFFFF); }
        }
        
        public int groupID
        {
            get { return (int) (_GID >> 32); }
        }

        internal EGID(int entityID, int groupID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, groupID);
        }
        
        public EGID(int entityID, ExclusiveGroup.ExclusiveGroupStruct  groupID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, groupID);
        }
        
        static long MAKE_GLOBAL_ID(int entityId, int groupId)
        {
            return (long)groupId << 32 | ((long)(uint)entityId & 0xFFFFFFFF);
        }

        public static implicit operator int(EGID id)
        {
            return id.entityID;
        }
        
        public static implicit operator long(EGID id)
        {
            return id._GID;
        }

        public bool Equals(long other)
        {
            return _GID == other;
        }

        public bool Equals(long x, long y)
        {
            return x == y;
        }

        public int GetHashCode(long obj)
        {
            return _GID.GetHashCode();
        }
    }
}