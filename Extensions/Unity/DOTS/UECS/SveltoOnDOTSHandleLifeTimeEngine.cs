#if UNITY_ECS

namespace Svelto.ECS.SveltoOnDOTS
{
    public interface ISveltoOnDOTSHandleLifeTimeEngine
    {
        EntityCommandBufferForSvelto entityCommandBuffer { set; }
    }

    public class SveltoOnDOTSHandleLifeTimeEngine<DOTSEntityComponent> : ISveltoOnDOTSHandleLifeTimeEngine,
        IReactOnRemove<DOTSEntityComponent>,
        IReactOnSwapEx<DOTSEntityComponent> where DOTSEntityComponent : unmanaged, IEntityComponentForDOTS
    {
        public void Remove(ref DOTSEntityComponent entityComponent, EGID egid)
        {
            ECB.DestroyEntity(entityComponent.dotsEntity);
        }

        EntityCommandBufferForSvelto ECB { get; set; }

        public EntityCommandBufferForSvelto entityCommandBuffer
        {
            set => ECB = value;
        }

        public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<DOTSEntityComponent> collection,
            ExclusiveGroupStruct _, ExclusiveGroupStruct toGroup)
        {
            var (buffer, entityIDs, _) = collection;

            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                ref var entityComponent = ref buffer[i];
                ECB.SetSharedComponent(entityComponent.dotsEntity, new DOTSSveltoGroupID(toGroup));

                ECB.SetComponent(entityComponent.dotsEntity, new DOTSSveltoEGID
                {
                    egid = new EGID(entityIDs[i], toGroup)
                });
            }
        }
    }
}
#endif