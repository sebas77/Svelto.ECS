#if UNITY_ECS
using Unity.Jobs;

namespace Svelto.ECS.SveltoOnDOTS
{
    /// <summary>
    /// Note: we don't want implementations of ISveltoDOTSSubmission
    /// to be able to add directly Submission or HandleLifeTime engines as
    /// the implementation are not aware of EnginesRoot so cannot satisfy Engines
    /// that implement IEngine interfaces 
    /// </summary>
    public interface ISveltoOnDOTSSubmission
    {
        void SubmitEntities(JobHandle jobHandle);
    }
}
#endif