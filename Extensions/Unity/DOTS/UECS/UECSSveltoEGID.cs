#if UNITY_ECS
using Unity.Entities;

namespace Svelto.ECS.Extensions.Unity
{
    public struct UECSSveltoEGID : IComponentData
    {
        public EGID egid;

        public UECSSveltoEGID(EGID egid) { this.egid = egid; }
    }

    public struct UECSSveltoGroupID : ISharedComponentData
    {
        public readonly ExclusiveGroupStruct group;

        public UECSSveltoGroupID(ExclusiveGroupStruct exclusiveGroup)
        {
            @group = exclusiveGroup;
        }

        public static implicit operator ExclusiveGroupStruct(UECSSveltoGroupID group)
        {
            return group.@group;
        }
    }

    public struct UpdateUECSEntityAfterSubmission : IComponentData
    {
        public EGID egid;

        public UpdateUECSEntityAfterSubmission(EGID egid) { this.egid = egid; }
    }

    public struct UECSEntityComponent : IEntityComponent
    {
        public Entity uecsEntity;
    }
}
#endif