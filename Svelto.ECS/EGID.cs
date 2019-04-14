#if !REAL_ID
using System;
using System.Collections.Generic;
#pragma warning disable 660,661

namespace Svelto.ECS
{
    public struct EGID:IEquatable<EGID>,IEqualityComparer<EGID>,IComparable<EGID>
    {
        readonly ulong _GID;

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
    }
}

#else

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#pragma warning disable 660,661

namespace Svelto.ECS
{
    public struct EGID:IEquatable<EGID>,IEqualityComparer<EGID>,IComparable<EGID>
    {
        readonly ulong _GID;
        const int idbits = 22; //one bit is reserved
        const int groupbits = 20;
        const int realidbits = 21;

        public EGID(uint entityID, ExclusiveGroup.ExclusiveGroupStruct groupID) : this()
        {
            DBC.ECS.Check.Require(entityID < bit21, "the entityID value is outside the range, max value: (2^22)-1");
            DBC.ECS.Check.Require(groupID < bit20, "the groupID value is outside the range");
            
            _GID = MAKE_GLOBAL_ID(entityID, groupID, 0, 1);
        }

        const uint bit21 = 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0001_1111_1111_1111_1111_1111;
        const uint bit22 = 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0011_1111_1111_1111_1111_1111;
        const uint bit20 = 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_1111_1111;

        public uint entityID
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (uint) (_GID & bit22); }
        }
        
        public ExclusiveGroup.ExclusiveGroupStruct groupID =>
            new ExclusiveGroup.ExclusiveGroupStruct((uint) ((_GID >> idbits) & bit20));

//         1    21       20     1     21         
//        | | realid | groupid |R| entityID |

        static ulong MAKE_GLOBAL_ID(uint entityId, uint groupId, uint realId, byte hasID)
        {
            var makeGlobalId = (((ulong)realId & bit21) << (idbits+groupbits)) | (((ulong)groupId & bit20) << idbits) | ((ulong)entityId & bit22);

            return makeGlobalId | (ulong) (hasID << idbits + groupbits + realidbits);
        }

        public static explicit operator uint(EGID id)
        {
            return id.entityID;
        }
        
        public static bool operator ==(EGID obj1, EGID obj2)
        {
            throw new NotSupportedException();
        }    
        
        public static bool operator !=(EGID obj1, EGID obj2)
        {
            throw new NotSupportedException();
        }

        public bool Equals(EGID other)
        {
            throw new NotSupportedException();
        }

        public bool Equals(EGID x, EGID y)
        {
            throw new NotSupportedException();
        }
        
        public int CompareTo(EGID other)
        {
            throw new NotSupportedException();
        }

//in the way it's used, ulong must be always the same for each id/group
public static explicit operator ulong(EGID id)
{
    throw new NotSupportedException();
}

        public int GetHashCode(EGID egid)
        {
            throw new NotSupportedException();
        }
        
        internal EGID(ulong GID) : this()
        {
            _GID = GID;
        }

        internal EGID(uint entityID, uint groupID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, groupID, 0, 1);
        }
        
        internal static EGID UPDATE_REAL_ID_AND_GROUP(EGID egid, uint toGroupID, uint realID)
        {
            if (egid.hasID == 0)
                return new EGID(MAKE_GLOBAL_ID(SAFE_ID(realID), toGroupID, realID, 0));
            
            return new EGID(MAKE_GLOBAL_ID(egid.entityID, toGroupID, realID, 1));
        }
        
        internal static EGID UPDATE_REAL_ID(EGID egid, uint realID)
        {
            if (egid.hasID == 0)
                return new EGID(MAKE_GLOBAL_ID(SAFE_ID(realID), egid.groupID, realID, 0));
            
            return new EGID(MAKE_GLOBAL_ID(egid.entityID, egid.groupID, realID, 1));
        }
        
        internal static EGID CREATE_WITHOUT_ID(uint toGroupID, uint realID) 
        {
            var _GID = MAKE_GLOBAL_ID(SAFE_ID(realID), toGroupID, realID, 0);
            return new EGID(_GID);
        }

        public byte hasID { get { return (byte) (_GID >> idbits + groupbits + realidbits); } }

        internal uint realID
        {
            get { return ((uint)(_GID >> idbits + groupbits)) & bit21; }
        }

        static uint SAFE_ID(uint u) { return u | (bit21 + 1);  }
    }
}
#endif