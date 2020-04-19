using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public interface IJobifiedEngine : IEngine
    {
        JobHandle Execute(JobHandle _jobHandle);
    }
}