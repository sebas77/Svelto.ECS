using Svelto.ECS.Schedulers;

namespace Svelto.ECS
{
    //This scheduler shouldn't be used in production and it's meant to be used for Unit Tests only
    public class SimpleEntitiesSubmissionScheduler : IEntitiesSubmissionScheduler
    {
        public void SubmitEntities()
        {
            _onTick.Invoke();
        }
        
        EnginesRoot.EntitiesSubmitter IEntitiesSubmissionScheduler.onTick
        {
            set => _onTick = value;
        }

        public void Dispose() { }

        EnginesRoot.EntitiesSubmitter _onTick;
    }
}