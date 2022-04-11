#if UNITY_ECS
using Svelto.Common;
using Svelto.ECS.Schedulers;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.SveltoOnDOTS
{
    /// <summary>
    /// This is a high level class to abstract the complexity of creating a Svelto ECS application that interacts
    /// with DOTS ECS. However this is designed to make it work almost out of the box, but it should be eventually
    /// substituted by project customized code.
    /// This is a JobifiedEngine and as such it expect to be ticked. Normally it must be executed in a
    /// SortedEnginesGroup as step that happens after the Svelto jobified engines run.
    ///
    /// The flow should be:
    /// Svelto (GameLogic) Engines Run first
    /// Then this Engine runs, which causes:
    /// Jobs to be completed (it's a sync point)
    /// Synchronizations engines to be executed (Svelto to DOTS ECS)
    /// Submission of Entities to be executed
    /// Svelto Add/Remove callbacks to be called
    /// ISubmissionEngines to be executed
    /// DOTS ECS engines to executed
    /// Synchronizations engines to be executed (DOTS ECS To Svelto)
    /// </summary>
    [Sequenced(nameof(JobifiedSveltoEngines.SveltoOnDOTS))]
    public class SveltoOnDOTSEnginesGroup : IJobifiedEngine
    {
        public SveltoOnDOTSEnginesGroup(EnginesRoot enginesRoot)
        {
            DBC.ECS.Check.Require(enginesRoot.scheduler is SimpleEntitiesSubmissionScheduler
                                , "The Engines root must use a EntitiesSubmissionScheduler scheduler implementation");

            CreateUnityECSWorldForSvelto(enginesRoot.scheduler as SimpleEntitiesSubmissionScheduler, enginesRoot);
        }

        public World world { get; private set; }

        public JobHandle Execute(JobHandle inputDeps)
        {
            //this is a sync point, there won't be pending jobs after this
            _sveltoDotsEntitiesSubmissionGroup.SubmitEntities(inputDeps);

            //Mixed explicit job dependency and internal automatic ECS dependency system
            //Write in to DOTS ECS entities so the DOTS ECS dependencies react on the components touched
            var handle = _syncSveltoToDotsGroup.Execute(default);

            //As long as pure DOTS ECS systems do not use external containers (like native arrays and so) the Unity
            //automatic dependencies system will guarantee that there won't be race conditions
            world.Update();

            //this svelto group of DOTS ECS SystemBase systems
            return _syncDotsToSveltoGroup.Execute(handle);
        }

        public string name => nameof(SveltoOnDOTSEnginesGroup);

        public void AddSveltoToDOTSEngine(SyncSveltoToDOTSEngine engine)
        {
            //it's a Svelto Engine/DOTS ECS SystemBase so it must be added in the DOTS ECS world AND svelto enginesRoot
            world.AddSystem(engine);
            _enginesRoot.AddEngine(engine);

            _syncSveltoToDotsGroup.Add(engine);
        }

        public void AddDOTSToSveltoEngine(SyncDOTSToSveltoEngine engine)
        {
            //it's a Svelto Engine/DOTS ECS SystemBase so it must be added in the DOTS ECS world AND svelto enginesRoot
            world.AddSystem(engine);
            _enginesRoot.AddEngine(engine);

            _syncDotsToSveltoGroup.Add(engine);
        }
        
        public void AddDOTSSubmissionEngine(SveltoOnDOTSHandleCreationEngine submissionEngine)
        {
            _sveltoDotsEntitiesSubmissionGroup.Add(submissionEngine);
            
            if (submissionEngine is IEngine enginesRootEngine)
                _enginesRoot.AddEngine(enginesRootEngine);
        }
        
        public void Dispose()
        {
            world.Dispose();
        }

        void CreateUnityECSWorldForSvelto(SimpleEntitiesSubmissionScheduler scheduler, EnginesRoot enginesRoot)
        {
            world = new World("Svelto<>DOTS world");

            var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);
            World.DefaultGameObjectInjectionWorld = world;

            //This is the DOTS ECS group that takes care of all the DOTS ECS systems that creates entities
            //it also submits Svelto entities
            _sveltoDotsEntitiesSubmissionGroup = new SveltoOnDOTSEntitiesSubmissionGroup(scheduler, enginesRoot);
            //This is the group that handles the DOTS ECS sync systems that copy the svelto entities values to DOTS ECS entities
            enginesRoot.AddEngine(_sveltoDotsEntitiesSubmissionGroup);
            world.AddSystem(_sveltoDotsEntitiesSubmissionGroup);
            _syncSveltoToDotsGroup = new SyncSveltoToDOTSGroup();
            enginesRoot.AddEngine(_syncSveltoToDotsGroup);
            _syncDotsToSveltoGroup = new SyncDOTSToSveltoGroup();
            enginesRoot.AddEngine(_syncDotsToSveltoGroup);
            //This is the group that handles the DOTS ECS sync systems that copy the DOTS ECS entities values to svelto entities
            //enginesRoot.AddEngine(new SveltoDOTS ECSEntitiesSubmissionGroup(scheduler, world));
            enginesRoot.AddEngine(this);

            _enginesRoot = enginesRoot;
        }

        EnginesRoot _enginesRoot;

        SveltoOnDOTSEntitiesSubmissionGroup _sveltoDotsEntitiesSubmissionGroup;
        SyncSveltoToDOTSGroup             _syncSveltoToDotsGroup;
        SyncDOTSToSveltoGroup             _syncDotsToSveltoGroup;
    }
}
#endif