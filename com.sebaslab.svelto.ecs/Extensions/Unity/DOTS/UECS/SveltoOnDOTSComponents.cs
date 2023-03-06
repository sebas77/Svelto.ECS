#if UNITY_ECS
using Unity.Entities;

namespace Svelto.ECS.SveltoOnDOTS
{
    /// <summary>
    /// If for some reason the user needs the DOTS entities to be grouped like the Svelto Entities, then this descriptor can be extended
    /// which will automatically enable the SveltoOnDOTSHandleLifeTimeEngine synchronization.
    /// This will also handle entities destruction.
    /// </summary>
    public class SveltoOnDotsSynchedEntityDescriptor: GenericEntityDescriptor<DOTSEntityComponent> { }
    
    public interface IEntityComponentForDOTS: IEntityComponent
    {
        public Entity dotsEntity { get; set; }
    }
    
    public struct DOTSEntityComponent:IEntityComponentForDOTS
    {
        public DOTSEntityComponent(Entity entity)
        {
            dotsEntity = entity;
        }

        public Entity dotsEntity { get; set; }
    }
    
    //DOTS COMPONENTS:
    
    /// <summary>
    /// DOTS component to keep track of the associated Svelto.ECS entity
    /// </summary>
    public struct DOTSSveltoEGID: IComponentData
    {
        public EGID egid;

        public DOTSSveltoEGID(EGID egid)
        {
            this.egid = egid;
        }
    }
    
    /// <summary>
    /// DOTS component to be able to query all the DOTS entities found in a Svelto.ECS group
    /// </summary>
    public readonly struct DOTSSveltoGroupID: ISharedComponentData
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
}
#endif