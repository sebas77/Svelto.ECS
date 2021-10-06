#if UNITY_ECS
using System.Collections;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Native;
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
    [DisableAutoCreation]
    public sealed class SveltoUECSEntitiesSubmissionGroup : SystemBase, IQueryingEntitiesEngine , IReactOnAddAndRemove<UECSEntityComponent>
                                                          , IReactOnSwap<UECSEntityComponent>, ISveltoUECSSubmission
    {
        public SveltoUECSEntitiesSubmissionGroup(SimpleEntitiesSubmissionScheduler submissionScheduler)
        {
            _submissionScheduler     = submissionScheduler;
            _engines                 = new FasterList<SubmissionEngine>();
            _afterSubmissionEngines  = new FasterList<IUpdateAfterSubmission>();
            _beforeSubmissionEngines = new FasterList<IUpdateBeforeSubmission>();
        }

        protected override void OnCreate()
        {
            _ECBSystem   = World.CreateSystem<SubmissionEntitiesCommandBufferSystem>();
            _entityQuery = GetEntityQuery(typeof(UpdateUECSEntityAfterSubmission));
        }

        public EntitiesDB                        entitiesDB          { get; set; }

        public void       Ready()    {  }
        
        public void Add(ref UECSEntityComponent entityComponent, EGID egid) { }

        public void Remove(ref UECSEntityComponent entityComponent, EGID egid)
        {
            _ECB.DestroyEntity(entityComponent.uecsEntity);
        }

        public void MovedTo(ref UECSEntityComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            _ECB.SetSharedComponent(entityComponent.uecsEntity, new UECSSveltoGroupID(egid.groupID));
        }
        
        public void Add(SubmissionEngine engine)
        {
            Svelto.Console.LogDebug($"Add Engine {engine} to the UECS world {_ECBSystem.World.Name}");
            
            _ECBSystem.World.AddSystem(engine);
            if (engine is IUpdateAfterSubmission afterSubmission)
                _afterSubmissionEngines.Add(afterSubmission);
            if (engine is IUpdateBeforeSubmission beforeSubmission)
                _beforeSubmissionEngines.Add(beforeSubmission);
            _engines.Add(engine);
        }

        public void SubmitEntities(JobHandle jobHandle)
        {
            if (_submissionScheduler.paused)
                return;

            using (var profiler = new PlatformProfiler("SveltoUECSEntitiesSubmissionGroup - PreSubmissionPhase"))
            {
                PreSubmissionPhase(ref jobHandle, profiler);

                //Submit Svelto Entities, calls Add/Remove/MoveTo that can be used by the IUECSSubmissionEngines
                using (profiler.Sample("Submit svelto entities"))
                {
                    _submissionScheduler.SubmitEntities();
                }

                AfterSubmissionPhase(profiler);
            }
        }

        public IEnumerator SubmitEntitiesAsync(JobHandle jobHandle, uint maxEntities)
        {
            if (_submissionScheduler.paused)
                yield break;

            using (var profiler = new PlatformProfiler("SveltoUECSEntitiesSubmissionGroup - PreSubmissionPhase"))
            {
                PreSubmissionPhase(ref jobHandle, profiler);

                var submitEntitiesAsync = _submissionScheduler.SubmitEntitiesAsync(maxEntities);

                //Submit Svelto Entities, calls Add/Remove/MoveTo that can be used by the IUECSSubmissionEngines
                while (true)
                {
                    using (profiler.Sample("Submit svelto entities async"))
                    {
                        submitEntitiesAsync.MoveNext();
                    }

                    if (submitEntitiesAsync.Current == true)
                    {
                        using (profiler.Yield())
                            yield return null;
                    }
                    else
                        break;
                }

                AfterSubmissionPhase(profiler);
            }
        }

        void PreSubmissionPhase(ref JobHandle jobHandle, PlatformProfiler profiler)
        {
            JobHandle BeforeECBFlushEngines()
            {
                JobHandle jobHandle = default;

                //execute submission engines and complete jobs because of this I don't need to do _ECBSystem.AddJobHandleForProducer(Dependency);

                for (var index = 0; index < _beforeSubmissionEngines.count; index++)
                {
                    ref var engine = ref _beforeSubmissionEngines[index];
                    using (profiler.Sample(engine.name))
                    {
                        jobHandle = JobHandle.CombineDependencies(jobHandle, engine.BeforeSubmissionUpdate(jobHandle));
                    }
                }

                return jobHandle;
            }

            using (profiler.Sample("Complete All Pending Jobs"))
            {
                jobHandle.Complete();
            }

            //prepare the entity command buffer to be used by the registered engines
            var entityCommandBuffer = _ECBSystem.CreateCommandBuffer();

            foreach (var system in _engines)
            {
                system.ECB = entityCommandBuffer;
            }

            _ECB = entityCommandBuffer;

            RemovePreviousMarkingComponents(entityCommandBuffer);

            using (profiler.Sample("Before Submission Engines"))
            {
                BeforeECBFlushEngines().Complete();
            }
        }

        void AfterSubmissionPhase(PlatformProfiler profiler)
        {
            JobHandle AfterECBFlushEngines()
            {
                JobHandle jobHandle = default;

                //execute submission engines and complete jobs because of this I don't need to do _ECBSystem.AddJobHandleForProducer(Dependency);
                for (var index = 0; index < _afterSubmissionEngines.count; index++)
                {
                    ref var engine = ref _afterSubmissionEngines[index];
                    using (profiler.Sample(engine.name))
                    {
                        jobHandle = JobHandle.CombineDependencies(jobHandle, engine.AfterSubmissionUpdate(jobHandle));
                    }
                }

                return jobHandle;
            }

            using (profiler.Sample("Flush Command Buffer"))
            {
                _ECBSystem.Update();
            }

            ConvertPendingEntities().Complete();

            using (profiler.Sample("After Submission Engines"))
            {
                AfterECBFlushEngines().Complete();
            }
        }

        void RemovePreviousMarkingComponents(EntityCommandBuffer ECB)
        {
            ECB.RemoveComponentForEntityQuery<UpdateUECSEntityAfterSubmission>(_entityQuery);
        }

        JobHandle ConvertPendingEntities()
        {
            if (_entityQuery.IsEmpty == false)
            {
                NativeEGIDMultiMapper<UECSEntityComponent> mapper =
                    entitiesDB.QueryNativeMappedEntities<UECSEntityComponent>(
                        entitiesDB.FindGroups<UECSEntityComponent>(), Allocator.TempJob);

                Entities.ForEach((Entity id, ref UpdateUECSEntityAfterSubmission egidComponent) =>
                {
                    mapper.Entity(egidComponent.egid).uecsEntity = id;
                }).ScheduleParallel();

                mapper.ScheduleDispose(Dependency);
            }

            return Dependency;
        }

        readonly SimpleEntitiesSubmissionScheduler   _submissionScheduler;
        SubmissionEntitiesCommandBufferSystem        _ECBSystem;
        readonly FasterList<SubmissionEngine>        _engines;
        readonly FasterList<IUpdateBeforeSubmission> _beforeSubmissionEngines;
        readonly FasterList<IUpdateAfterSubmission>  _afterSubmissionEngines;

        [DisableAutoCreation]
        class SubmissionEntitiesCommandBufferSystem : EntityCommandBufferSystem { }

        protected override void OnUpdate() { }

        EntityQuery         _entityQuery;
        EntityCommandBuffer _ECB;
    }

    public interface ISveltoUECSSubmission
    {
        void Add(SubmissionEngine engine);

        void        SubmitEntities(JobHandle jobHandle);
        IEnumerator SubmitEntitiesAsync(JobHandle jobHandle, uint maxEntities);
    }
}
#endif