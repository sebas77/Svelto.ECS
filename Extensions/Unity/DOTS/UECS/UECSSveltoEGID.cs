#if UNITY_ECS
using Unity.Entities;

namespace Svelto.ECS.Extensions.Unity
{
    public struct UECSSveltoEGID : IComponentData
    {
        public EGID egid;
    }

    public struct UECSSveltoGroupID : ISharedComponentData
    {
        public readonly uint group;
        
        public UECSSveltoGroupID(uint exclusiveGroup) { @group = exclusiveGroup; }
        
        public static implicit operator ExclusiveGroupStruct(UECSSveltoGroupID group)
        {
            return new ExclusiveGroupStruct(group.@group);
        }
    }
}
#endif