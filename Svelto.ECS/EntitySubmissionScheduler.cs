using Svelto.WeakEvents;

namespace Svelto.ECS.Schedulers
{
    public abstract class EntitySubmissionScheduler
    {
        public abstract void Schedule(WeakAction submitEntityViews);
    }
}