#if UNITY_ECS
using Unity.Entities;

namespace Svelto.ECS.Extensions.Unity
{
    public interface IUECSSubmissionEngine : IJobifiedEngine
    {
        EntityCommandBuffer ECB { get; set;}
        EntityManager EM { get; set;}
    }
}
#endif