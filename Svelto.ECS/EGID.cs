using DBC;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct EGID
    {
        int _GID;
        
        public int GID
        {
            get { return _GID; }
        }
        
        public int ID
        {
            get { return _GID & 0xFFFFFF; }
        }
        
        public int group
        {
            get { return (int) ((_GID & 0xFF000000) >> 24); }
        }

        public EGID(int entityID, int groupID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, groupID);
        }
        
        public EGID(int entityID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, ExclusiveGroups.StandardEntity);
        }

        int MAKE_GLOBAL_ID(int entityId, int groupId)
        {
#if DEBUG && !PROFILER
            Check.Require(entityId <= 0xFFFFFF);
            Check.Require(groupId <= 0xFF);
#endif
            return entityId | groupId << 24;
        }

        public bool IsEqualTo(EGID otherGID)
        {
            return otherGID._GID == _GID;
        }
    }
}