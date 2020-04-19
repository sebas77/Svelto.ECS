#if UNITY_ECS
using Svelto.Common;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    [Sequenced(nameof(JobifiedSveltoEngines.CopySveltoToUECSEnginesGroup))]
    [DisableAutoCreation]
    public class CopySveltoToUECSEnginesGroup : ComponentSystemGroup, IJobifiedEngine
    {
        public JobHandle Execute(JobHandle _jobHandle)
        {
            foreach (var engine in Systems)
                (engine as ICopySveltoToUECSEngine).jobHandle = _jobHandle;
            
            Update();
            
            return _jobHandle;
        }

        readonly SimulationSystemGroup _simulationSystemGroup;
    }

    public interface ICopySveltoToUECSEngine:IEngine
    {
        JobHandle jobHandle { set; }
    }
}
#endif