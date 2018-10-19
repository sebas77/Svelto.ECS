using Svelto.WeakEvents;

namespace Svelto.ECS.Schedulers
{
    public interface IEntitySubmissionScheduler
    {
        WeakAction onTick { set; }
    }
}