#if UNITY_ECS
#if !UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP_RUNTIME_WORLD && !UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
#error SveltoOnDOTS required the user to take over the DOTS world control and explicitly create it. UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP must be defined
#endif
using System;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.SveltoOnDOTS
{
    /// <summary>
    ///     SveltoDOTS ECSEntitiesSubmissionGroup extends the _submissionScheduler responsibility to integrate the
    ///     submission of Svelto entities with the submission of DOTS ECS entities using DOTSOperationsForSvelto.
    ///     As there is just one submissionScheduler per enginesRoot, there should be only one SveltoDOTS
    ///     ECSEntitiesSubmissionGroup.
    ///     initialise DOTS ECS/Svelto systems/engines that handles DOTS ECS entities structural changes.
    ///     Flow:
    ///     Complete all the jobs used as input dependencies (this is a sync point)
    ///     Svelto entities are submitted
    ///     Svelto Add and remove callback are called
    ///     ISveltoOnDOTSStructuralEngine can use DOTSOperationsForSvelto in their add/remove/moove callbacks
    /// </summary>
    [DisableAutoCreation]
    public sealed partial class SveltoOnDOTSEntitiesSubmissionGroup: SystemBase, IQueryingEntitiesEngine, ISveltoOnDOTSSubmission 
    {
        public SveltoOnDOTSEntitiesSubmissionGroup(EntitiesSubmissionScheduler submissionScheduler)
        {
            _submissionScheduler = submissionScheduler;
            _structuralEngines = new FasterList<ISveltoOnDOTSStructuralEngine>();
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void SubmitEntities(JobHandle jobHandle)
        {
            if (_submissionScheduler.paused == true || World.EntityManager == default)
                return;

            using (var profiler = new PlatformProfiler("SveltoDOTSEntitiesSubmissionGroup"))
            {
                using (profiler.Sample("Complete All Pending Jobs"))
                {
                    jobHandle.Complete(); //sync-point
#if UNITY_ECS_100
                    EntityManager.CompleteAllTrackedJobs();
#else                    
                    EntityManager.CompleteAllJobs();
#endif
                }

                //Submit Svelto Entities, calls Add/Remove/MoveTo that can be used by the DOTS ECSSubmissionEngines
                _submissionScheduler.SubmitEntities();

                foreach (var engine in _structuralEngines)
                    engine.OnPostSubmission();

                _dotsOperationsForSvelto.Complete();
            }
        }

        public void Add(ISveltoOnDOTSStructuralEngine engine)
        {
            _structuralEngines.Add(engine);
            if (_isReady == true)
            {
                engine.DOTSOperations = _dotsOperationsForSvelto;
                engine.OnOperationsReady();
            }
        }

        protected override void OnCreate()
        {
            unsafe
            {
                _jobHandle = (JobHandle*) MemoryUtilities.NativeAlloc((uint)MemoryUtilities.SizeOf<JobHandle>(), Allocator.Persistent);
                _dotsOperationsForSvelto = new DOTSOperationsForSvelto(World.EntityManager, _jobHandle);
                _isReady = true;
            
                //initialise engines field while world was null
                foreach (var engine in _structuralEngines)
                {
                    engine.DOTSOperations = _dotsOperationsForSvelto;
                    engine.OnOperationsReady();
                }
            }
        }

        protected override void OnDestroy()
        {
            unsafe
            {
                base.OnDestroy();
            
                MemoryUtilities.NativeFree((IntPtr)_jobHandle, Allocator.Persistent);
            }
        }

        protected override void OnUpdate()
        {
            throw new NotSupportedException("if this is called something broke the original design");
        }

        readonly FasterList<ISveltoOnDOTSStructuralEngine> _structuralEngines;
        readonly EntitiesSubmissionScheduler _submissionScheduler;
        DOTSOperationsForSvelto _dotsOperationsForSvelto;
        bool _isReady;
        unsafe JobHandle* _jobHandle;
    }
}
#endif