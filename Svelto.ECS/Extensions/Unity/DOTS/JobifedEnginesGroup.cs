#if UNITY_2019_2_OR_NEWER
using Svelto.Common;
using Svelto.DataStructures;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public interface IJobifiedEngine : IEngine
    {
        JobHandle Execute(JobHandle _jobHandle);
        
        string name { get; }
    }
    
    public interface IJobifiedGroupEngine : IJobifiedEngine
    { }
    
    public interface IJobifiedEngine<T> : IEngine
    {
        JobHandle Execute(JobHandle _jobHandle, ref T _param);
        
        string name { get; }
    }
    
    public interface IJobifiedGroupEngine<T> : IJobifiedEngine<T>
    {
    }
    /// <summary>
    /// Note unsorted jobs run in parallel
    /// </summary>
    /// <typeparam name="Interface"></typeparam>
    public abstract class JobifedEnginesGroup<Interface> : IJobifiedGroupEngine where Interface : class, IJobifiedEngine
    {
        protected JobifedEnginesGroup(FasterList<Interface> engines)
        {
            _name = "JobifiedEnginesGroup - "+this.GetType().Name;
            _engines         = engines;
        }

        public JobHandle Execute(JobHandle inputHandles)
        {
            var engines = _engines;
            JobHandle combinedHandles = inputHandles;
            using (var profiler = new PlatformProfiler(_name))
            {
                for (var index = 0; index < engines.count; index++)
                {
                    ref var engine = ref engines[index];
                    using (profiler.Sample(engine.name))
                    {
                        combinedHandles = JobHandle.CombineDependencies(combinedHandles, engine.Execute(inputHandles));
                    }
                }
            }

            return combinedHandles;
        }

        public string name => _name;

        readonly FasterReadOnlyList<Interface> _engines;
        readonly bool                          _completeEachJob;
        readonly string                        _name;
    }
    
    public abstract class JobifedEnginesGroup<Interface, Param>: IJobifiedGroupEngine<Param> where Interface : class, IJobifiedEngine<Param>
    {
        protected JobifedEnginesGroup(FasterList<Interface> engines)
        {
            _name = "JobifiedEnginesGroup - "+this.GetType().Name;
            _engines         = engines;
        }

        public JobHandle Execute(JobHandle combinedHandles, ref Param _param)
        {
            var engines = _engines;
            using (var profiler = new PlatformProfiler(_name))
            {
                for (var index = 0; index < engines.count; index++)
                {
                    var engine = engines[index];
                    using (profiler.Sample(engine.name)) combinedHandles =
                        JobHandle.CombineDependencies(combinedHandles, engine.Execute(combinedHandles, ref _param));
                }
            }

            return combinedHandles;
        }
        
        public string name => _name;
        
        readonly string _name;

        readonly FasterReadOnlyList<Interface> _engines;
    }
}
#endif