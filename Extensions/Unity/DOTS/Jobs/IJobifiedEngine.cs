#if UNITY_JOBS
using Unity.Jobs; 

namespace Svelto.ECS.SveltoOnDOTS
{
    public interface IJobifiedEngine<T> : IEngine
    {
        JobHandle Execute(JobHandle inputDeps, ref T _param);
        
        string name { get; }
    }

    public interface IJobifiedEngine : IEngine
    {
        JobHandle Execute(JobHandle inputDeps);

        string name { get; }
    }
    
    public interface IJobifiedGroupEngine<T> : IJobifiedEngine<T>
    { }
}

#endif