using System;

namespace Svelto.ECS.Schedulers
{
    public interface IEntitySubmissionScheduler: IDisposable
    {
        EnginesRoot.EntitiesSubmitter onTick { set; }
    }
}