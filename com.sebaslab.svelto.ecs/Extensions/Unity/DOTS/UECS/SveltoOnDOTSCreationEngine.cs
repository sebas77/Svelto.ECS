#if UNITY_ECS
namespace Svelto.ECS.SveltoOnDOTS
{
    /// <summary>
    /// SubmissionEngine is a dedicated DOTS ECS Svelto.ECS engine that allows using the DOTS ECS
    /// EntityCommandBuffer for fast creation of DOTS entities
    /// </summary>
    public interface ISveltoOnDOTSStructuralEngine
    {
        DOTSOperationsForSvelto DOTSOperations { get; set; }

        string name { get; }
        
        void OnPostSubmission();
    }
}
#endif