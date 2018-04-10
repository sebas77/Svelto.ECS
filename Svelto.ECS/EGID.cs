using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct EGID
    {
        long _GID;
        
        public long GID
        {
            get { return _GID; }
        }
        
        public int ID
        {
            get { return (int) (_GID & 0xFFFFFFFF); }
        }
        
        public int group
        {
            get { return (int) (_GID >> 32); }
        }

        public EGID(int entityID, int groupID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, groupID);
        }
        
        public EGID(int entityID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, ExclusiveGroups.StandardEntity);
        }

        static long MAKE_GLOBAL_ID(int entityId, int groupId)
        {
            return entityId | (long)groupId << 32;
        }

        public bool IsEqualTo(EGID otherGID)
        {
            return otherGID._GID == _GID;
        }
    }
}