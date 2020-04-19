using Svelto.DataStructures;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public abstract class JobifedEnginesGroup<Interface>
        where Interface : class, IJobifiedEngine
    {
        protected JobifedEnginesGroup(FasterReadOnlyList<Interface> engines, bool completeEachJob = false)
        {
            _engines         = engines;
            _completeEachJob = completeEachJob;
        }

        public JobHandle Execute(JobHandle combinedHandles)
        {
            var fasterReadOnlyList = _engines;
            for (var index = 0; index < fasterReadOnlyList.Count; index++)
            {
                var engine = fasterReadOnlyList[index];
                combinedHandles = JobHandle.CombineDependencies(combinedHandles, engine.Execute(combinedHandles));
                if (_completeEachJob) combinedHandles.Complete();
            }

            return combinedHandles;
        }

        readonly FasterReadOnlyList<Interface> _engines;
        readonly bool                          _completeEachJob;
    }
}