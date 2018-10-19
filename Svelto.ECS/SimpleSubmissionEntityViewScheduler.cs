using Svelto.ECS.Schedulers;
using Svelto.WeakEvents;

namespace Svelto.ECS
{
    //This scheduler shouldn't be used in production and it's meant to be 
    //used for Unit Tests only
    public class SimpleSubmissionEntityViewScheduler : IEntitySubmissionScheduler
    {
        public void SubmitEntities()
        {
            onTick.Invoke();
        }
        
        public WeakAction onTick { set; private get; }
    }
}