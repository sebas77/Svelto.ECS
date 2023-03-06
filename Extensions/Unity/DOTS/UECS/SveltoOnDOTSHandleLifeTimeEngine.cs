#if UNITY_ECS
using Unity.Collections;
using Unity.Entities;

namespace Svelto.ECS.SveltoOnDOTS
{
    /// <summary>
    /// Automatic Svelto Group -> DOTS archetype synchronization when necessary
    /// </summary>
    /// <typeparam name="DOTSEntityComponent"></typeparam>
    public class SveltoOnDOTSHandleLifeTimeEngine<DOTSEntityComponent>: ISveltoOnDOTSStructuralEngine, IReactOnRemoveEx<DOTSEntityComponent>,
            IReactOnSwapEx<DOTSEntityComponent> where DOTSEntityComponent : unmanaged, IEntityComponentForDOTS
    {
        public void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<DOTSEntityComponent> entities, ExclusiveGroupStruct groupID)
        {
            //todo burstify all of this if DOTS 1.0 is on
            
            var (buffer, _) = entities;

            var nativeArray = new NativeArray<Entity>((int)(rangeOfEntities.end - rangeOfEntities.start), Allocator.Temp);

            int counter = 0;
            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
                nativeArray[counter++] = buffer[i].dotsEntity;

            DOTSOperations.DestroyEntitiesBatched(nativeArray);
        }

        public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<DOTSEntityComponent> entities,
            ExclusiveGroupStruct _, ExclusiveGroupStruct toGroup)
        {
            //todo burstify all of this if DOTS 1.0 is on

            var (buffer, ids, _) = entities;

            var nativeArray = new NativeArray<Entity>((int)(rangeOfEntities.end - rangeOfEntities.start), Allocator.Temp);

            int counter = 0;
            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
                nativeArray[counter++] = buffer[i].dotsEntity;

            DOTSOperations.SetSharedComponentBatched(nativeArray, new DOTSSveltoGroupID(toGroup));
            
            counter = 0;
            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
                DOTSOperations.SetComponent(nativeArray[counter++], new DOTSSveltoEGID(new EGID(ids[i], toGroup)));
        }

        public void OnOperationsReady() {}
        public void OnPostSubmission() {}

        public DOTSOperationsForSvelto DOTSOperations { get; set; }
        public string name => nameof(SveltoOnDOTSHandleLifeTimeEngine<DOTSEntityComponent>);
    }
}
#endif