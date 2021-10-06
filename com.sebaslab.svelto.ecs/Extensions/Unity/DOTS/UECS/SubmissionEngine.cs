#if UNITY_ECS
using Svelto.Common;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public interface IUpdateBeforeSubmission
    {
        JobHandle BeforeSubmissionUpdate(JobHandle jobHandle);
        string    name { get; }
    }

    public interface IUpdateAfterSubmission
    {
        JobHandle AfterSubmissionUpdate(JobHandle jobHandle);
        string    name { get; }
    }

    public abstract class SubmissionEngine : SystemBase, IEngine
    {
        public    EntityCommandBuffer ECB { get; internal set; }
        
        protected sealed override void OnUpdate() {}

        public string name => TypeToString.Name(this);
    }
}
#endif