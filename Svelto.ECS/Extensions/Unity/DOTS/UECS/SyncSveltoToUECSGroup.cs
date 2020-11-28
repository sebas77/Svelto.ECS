#if UNITY_ECS
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public class SyncSveltoToUECSGroup : JobifiedEnginesGroup<SyncSveltoToUECSEngine>
    {
    }

    public abstract class SyncSveltoToUECSEngine : SystemBase, IJobifiedEngine
    {
        public JobHandle Execute(JobHandle inputDeps)
        {
            Dependency = JobHandle.CombineDependencies(Dependency, inputDeps);
            
            Update();

            return Dependency;
        }

        public abstract    string name       { get; }
    }
}
#endif