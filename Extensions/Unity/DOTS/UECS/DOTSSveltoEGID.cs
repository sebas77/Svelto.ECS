#if UNITY_ECS
using Unity.Entities;

namespace Svelto.ECS.SveltoOnDOTS
{
    /// <summary>
    /// DOTS component to keep track of the associated Svelto.ECS entity
    /// </summary>
    public struct DOTSSveltoEGID : IComponentData
    {
        public EGID egid;

        public DOTSSveltoEGID(EGID egid) { this.egid = egid; }
    }

    /// <summary>
    /// DOTS component to be able to query all the DOTS entities found in a Svelto.ECS group
    /// </summary>
    public readonly struct DOTSSveltoGroupID : ISharedComponentData
    {
        readonly ExclusiveGroupStruct group;

        public DOTSSveltoGroupID(ExclusiveGroupStruct exclusiveGroup)
        {
            @group = exclusiveGroup;
        }

        public static implicit operator ExclusiveGroupStruct(DOTSSveltoGroupID group)
        {
            return group.@group;
        }
    }

    struct DOTSEntityToSetup : ISharedComponentData
    {
        internal readonly ExclusiveGroupStruct group;

        public DOTSEntityToSetup(ExclusiveGroupStruct exclusiveGroup)
        {
            @group = exclusiveGroup;
        }
    }

  public interface IEntityComponentForDOTS: IEntityComponent
    {
        public Entity dotsEntity { get; set; }
    }
    
    
    public struct DOTSEntityComponent:IEntityComponentForDOTS
    {
        public Entity dotsEntity { get; set; }
    }
}
#endif