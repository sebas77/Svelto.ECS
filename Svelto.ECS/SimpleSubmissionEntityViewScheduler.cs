using System;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS
{
    //This scheduler shouldn't be used in production and it's meant to be used for Unit Tests only
    public class SimpleSubmissionEntityViewScheduler : IEntitySubmissionScheduler
    {
        public void SubmitEntities()
        {
            _onTick.Invoke();
        }
        
        EnginesRoot.EntitiesSubmitter IEntitySubmissionScheduler.onTick
        {
            set => _onTick = value;
        }
        
        EnginesRoot.EntitiesSubmitter _onTick;
    }
}