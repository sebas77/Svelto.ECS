#if UNITY_ECS
using Unity.Collections;
using Unity.Entities;

namespace Svelto.ECS.SveltoOnDOTS
{
    /// <summary>
    /// Automatic Svelto Group -> DOTS archetype synchronization when necessary
    /// </summary>
    /// <typeparam name="DOTSEntityComponent"></typeparam>
    class SveltoOnDOTSHandleLifeTimeEngine<DOTSEntityComponent>: ISveltoOnDOTSStructuralEngine, IReactOnRemoveEx<DOTSEntityComponent>,
            IReactOnSwapEx<DOTSEntityComponent> where DOTSEntityComponent : unmanaged, IEntityComponentForDOTS
    {
        public void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<DOTSEntityComponent> entities, ExclusiveGroupStruct groupID)
        {
            var (buffer, _) = entities;

            var nativeArray = new NativeArray<Entity>((int)(rangeOfEntities.end - rangeOfEntities.start), Allocator.Temp);

            //todo this could be burstified or memcpied
            int counter = 0;
            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
                nativeArray[counter++] = buffer[i].dotsEntity;

            DOTSOperations.DestroyEntitiesBatched(nativeArray);
        }

        public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<DOTSEntityComponent> entities,
            ExclusiveGroupStruct _, ExclusiveGroupStruct toGroup)
        {
            var (buffer, _) = entities;

            var nativeArray = new NativeArray<Entity>((int)(rangeOfEntities.end - rangeOfEntities.start), Allocator.Temp);

            //todo this could be burstified or memcpied
            int counter = 0;
            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
                nativeArray[counter++] = buffer[i].dotsEntity;

            DOTSOperations.SetSharedComponentBatched(nativeArray, new DOTSSveltoGroupID(toGroup));
        }
        
        public void OnPostSubmission() { }

        public DOTSOperationsForSvelto DOTSOperations { get; set; }
        public string name => nameof(SveltoOnDOTSHandleLifeTimeEngine<DOTSEntityComponent>);
    }
}
#endif