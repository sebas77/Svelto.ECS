using System;

namespace Svelto.ECS.Schedulers
{
    public interface IEntitiesSubmissionScheduler: IDisposable
    {
        EnginesRoot.EntitiesSubmitter onTick { set; }
    }
}