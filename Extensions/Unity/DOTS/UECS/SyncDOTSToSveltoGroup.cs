#if UNITY_ECS
using Svelto.Common;
using Svelto.DataStructures;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.SveltoOnDOTS
{
    public class SyncDOTSToSveltoGroup : UnsortedJobifiedEnginesGroup<ISyncDOTSToSveltoEngine> {}

    public class SortedSyncDOTSToSveltoGroup<T_Order> : SortedJobifiedEnginesGroup<ISyncDOTSToSveltoEngine, T_Order>,
        ISyncDOTSToSveltoEngine where T_Order : struct, ISequenceOrder
    {
        public SortedSyncDOTSToSveltoGroup(FasterList<ISyncDOTSToSveltoEngine> engines) : base(engines)
        {
        }
    }

    public interface ISyncDOTSToSveltoEngine : IJobifiedEngine { }

    public abstract partial class SyncDOTSToSveltoEngine : SystemBase, ISyncDOTSToSveltoEngine
    {
        //The dependency returned is enough for the Svelto Engines running after this to take in consideration
        //the Systembase jobs. The svelto engines do not need to take in consideration the new dependencies created
        //by the World.Update because those are independent and are needed only by the next World.Update() jobs
        public JobHandle Execute(JobHandle inputDeps)
        {
            _inputDeps = inputDeps;

            Update(); //this complete the previous frame jobs so dependency cannot be modified atr this point

            return Dependency;
        }

        //TODO if this is correct must change SyncDOTSToSveltoGroup too
        protected sealed override void OnUpdate()
        {
            //SysteBase jobs that will use this Dependency will wait for inputDeps to be completed before to execute
            Dependency = JobHandle.CombineDependencies(Dependency, _inputDeps);
            
            OnSveltoUpdate();
        }

        protected abstract void OnSveltoUpdate();

        public abstract string name { get; }
        
        JobHandle _inputDeps;
    }
    
    public abstract partial class SortedSyncDOTSToSveltoEngine : SystemBase, ISyncDOTSToSveltoEngine
    {
        //The dependency returned is enough for the Svelto Engines running after this to take in consideration
        //the Systembase jobs. The svelto engines do not need to take in consideration the new dependencies created
        //by the World.Update because those are independent and are needed only by the next World.Update() jobs
        public JobHandle Execute(JobHandle inputDeps)
        {
            _inputDeps = inputDeps;

            Update(); //this complete the previous frame jobs so dependency cannot be modified atr this point

            return Dependency;
        }

        //TODO if this is correct must change SyncDOTSToSveltoGroup too
        protected sealed override void OnUpdate()
        {
            //SysteBase jobs that will use this Dependency will wait for inputDeps to be completed before to execute
            Dependency = JobHandle.CombineDependencies(Dependency, _inputDeps);
            
            OnSveltoUpdate();
        }

        protected abstract void OnSveltoUpdate();

        public abstract string name { get; }
        
        JobHandle _inputDeps;
    }
}
#endif