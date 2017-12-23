using Svelto.WeakEvents;

namespace Svelto.ECS.Schedulers
{
    public abstract class EntityViewSubmissionScheduler
    {
        abstract public void Schedule(WeakAction submitEntityViews);
    }
}