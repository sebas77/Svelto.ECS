#if UNITY_ECS
using Svelto.ECS.Schedulers;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    /// <summary>
    /// Group of UECS/Svelto SystemBase engines that creates UECS entities.
    /// Svelto entities are submitted
    /// Svelto Add and remove callback are called
    /// OnUpdate of the systems are called
    /// finally the UECS command buffer is flushed
    /// Note: I cannot use Unity ComponentSystemGroups nor I can rely on the SystemBase Dependency field to
    /// solve external dependencies. External dependencies are tracked, but only linked to the UECS components operations
    /// With Dependency I cannot guarantee that an external container is used before previous jobs working on it are completed
    /// </summary>
    public class SveltoUECSEntitiesSubmissionGroup : JobifiedEnginesGroup<IUECSSubmissionEngine>
    {
        public SveltoUECSEntitiesSubmissionGroup
            (ISimpleEntitiesSubmissionScheduler submissionScheduler, World UECSWorld)
        {
            _submissionScheduler = submissionScheduler;
            _ECBSystem           = UECSWorld.CreateSystem<SubmissionEntitiesCommandBufferSystem>();
        }

        public new void Execute(JobHandle jobHandle)
        {
            //Sync Point as we must be sure that jobs that create/swap/remove entities are done
            jobHandle.Complete();

            if (_submissionScheduler.paused)
                return;

            //prepare the entity command buffer to be used by the registered engines
            var entityCommandBuffer = _ECBSystem.CreateCommandBuffer();

            foreach (var system in _engines)
            {
                system.ECB = entityCommandBuffer;
                system.EM  = _ECBSystem.EntityManager;
            }

            //Submit Svelto Entities, calls Add/Remove/MoveTo that can be used by the IUECSSubmissionEngines
            _submissionScheduler.SubmitEntities();

            //execute submission engines and complete jobs
            base.Execute(default).Complete();

            //flush command buffer
            _ECBSystem.Update();
        }

        readonly ISimpleEntitiesSubmissionScheduler    _submissionScheduler;
        readonly SubmissionEntitiesCommandBufferSystem _ECBSystem;

        [DisableAutoCreation]
        class SubmissionEntitiesCommandBufferSystem : EntityCommandBufferSystem { }
    }
}
#endif