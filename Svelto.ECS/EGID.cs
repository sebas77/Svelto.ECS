using System;
using System.Collections.Generic;

namespace Svelto.ECS
{
    public struct EGID:IEquatable<long>,IEqualityComparer<long>
    {
        long _GID;
        
        public long GID
        {
            get { return _GID; }
        }
        
        public int entityID
        {
            get { return (int) (_GID & 0xFFFFFFFF); }
        }
        
        public int groupID
        {
            get { return (int) (_GID >> 32); }
        }

        public EGID(int entityID, int groupID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, groupID);
        }
        
        public EGID(int entityID, ExclusiveGroup groupID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, (int) groupID);
        }
        
        static long MAKE_GLOBAL_ID(int entityId, int groupId)
        {
            return (long)groupId << 32 | ((long)(uint)entityId & 0xFFFFFFFF);
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