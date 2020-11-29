using System;

namespace Svelto.ECS.Schedulers
{
    public interface IEntitiesSubmissionScheduler: IDisposable
    {
        bool paused { get; set; }
    }
    
    public abstract class EntitiesSubmissionScheduler: IEntitiesSubmissionScheduler
    {
        protected internal abstract EnginesRoot.EntitiesSubmitter onTick { set; }
        public abstract    void                          Dispose();
        public abstract    bool                          paused { get; set; }
    }
    
    public abstract class ISimpleEntitiesSubmissionScheduler: EntitiesSubmissionScheduler
    {
        public abstract void SubmitEntities();
    }
}