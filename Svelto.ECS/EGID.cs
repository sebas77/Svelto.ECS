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
        
        public EGID(int entityID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, ExclusiveGroups.StandardEntity);
        }

        static long MAKE_GLOBAL_ID(int entityId, int groupId)
        {
            return (long)groupId << 32 | (((long)(uint)entityId) & 0xFFFFFFFF);
        }
    }
}