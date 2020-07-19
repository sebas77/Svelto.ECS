#if UNITY_ECS
using Svelto.Common;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    [Sequenced(nameof(JobifiedSveltoEngines.PureUECSSystemsGroup))]
    [DisableAutoCreation]
    public class PureUECSSystemsGroup : IJobifiedEngine
    {
        public PureUECSSystemsGroup(World world)
        {
            _world = world;
        }

        public JobHandle Execute(JobHandle _jobHandle)
        {
            _world.Update();

            return _jobHandle;
        }

        public string name => nameof(PureUECSSystemsGroup);

        readonly World _world;
    }
}
#endif