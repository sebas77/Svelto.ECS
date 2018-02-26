using Svelto.ECS.Schedulers;
using Svelto.WeakEvents;

namespace Svelto.ECS
{
    //This scheduler shouldn't be used in production and it's meant to be 
    //used for Unit Tests only
    public class SimpleSubmissionEntityViewScheduler : EntitySubmissionScheduler
    {
        public void SubmitEntities()
        {
            _submitEntityViews.Invoke();
        }
            
        public override void Schedule(WeakAction submitEntityViews)
        {
            _submitEntityViews = submitEntityViews;
        }
            
        WeakAction _submitEntityViews;
    }
}