using Svelto.WeakEvents;

namespace Svelto.ECS.Schedulers
{
    public abstract class EntitySubmissionScheduler
    {
        abstract public void Schedule(WeakAction submitEntityViews);
    }
}