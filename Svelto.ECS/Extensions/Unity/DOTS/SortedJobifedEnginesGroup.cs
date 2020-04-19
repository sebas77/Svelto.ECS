#if UNITY_2019_1_OR_NEWER
using Svelto.DataStructures;
using Unity.Jobs;
using Svelto.Common;

namespace Svelto.ECS.Extensions.Unity
{
    public abstract class SortedJobifedEnginesGroup<Interface, SequenceOrder>
        where SequenceOrder : struct, ISequenceOrder where Interface : class, IJobifiedEngine
    {
        protected SortedJobifedEnginesGroup(FasterReadOnlyList<Interface> engines)
        {
            _instancedSequence = new Sequence<Interface, SequenceOrder>(engines);
        }

        public JobHandle Execute(JobHandle combinedHandles)
        {
            var fasterReadOnlyList = _instancedSequence.items;
            for (var index = 0; index < fasterReadOnlyList.Count; index++)
            {
                var engine = fasterReadOnlyList[index];
                combinedHandles = JobHandle.CombineDependencies(combinedHandles, engine.Execute(combinedHandles));
            }

            return combinedHandles;
        }

        readonly Sequence<Interface, SequenceOrder> _instancedSequence;
    }
}
#endif