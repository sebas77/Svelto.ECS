#if UNITY_ECS
using Unity.Jobs;

namespace Svelto.ECS.SveltoOnDOTS
{
    /// <summary>
    /// this interface exists to allow the user to submig entities explicitly
    /// </summary>
    public interface ISveltoOnDOTSSubmission
    {
        void SubmitEntities(JobHandle jobHandle);
    }
}
#endif