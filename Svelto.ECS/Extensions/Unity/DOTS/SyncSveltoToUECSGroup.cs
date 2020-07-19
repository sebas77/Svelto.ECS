#if UNITY_ECS
using Svelto.Common;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    [Sequenced(nameof(JobifiedSveltoEngines.CopySveltoToUECSEnginesGroup))]
    [DisableAutoCreation]
    public class SyncSveltoToUECSGroup : ComponentSystemGroup, IJobifiedEngine
    {
        public JobHandle Execute(JobHandle _jobHandle)
        {
            foreach (var engine in Systems)
                (engine as SyncSveltoToUECSEngine).externalHandle = _jobHandle;
            
            Update();
            
            return _jobHandle;
        }
        
        public string name => nameof(SyncSveltoToUECSGroup);

        readonly SimulationSystemGroup _simulationSystemGroup;
    }
    
    public abstract class SyncSveltoToUECSEngine : SystemBase, IEngine
    {
        internal JobHandle externalHandle;
        protected abstract void Execute();

        protected sealed override void OnUpdate()
        {
            Dependency = JobHandle.CombineDependencies(Dependency, externalHandle);
            Execute();
        }
    }
}
#endif