#if UNITY_ECS
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public class SyncUECSToSveltoGroup : UnsortedJobifiedEnginesGroup<SyncUECSToSveltoEngine>
    {
        
    }

    public abstract class SyncUECSToSveltoEngine : SystemBase, IJobifiedEngine
    {
        public JobHandle Execute(JobHandle inputDeps)
        {
            Dependency = JobHandle.CombineDependencies(Dependency, inputDeps);
            
            Update();

            return Dependency;
        }

        public abstract string name { get; }
    }
}
#endif