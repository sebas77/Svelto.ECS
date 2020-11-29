using Svelto.ECS.Schedulers;

namespace Svelto.ECS.Schedulers
{
    //This scheduler shouldn't be used in production and it's meant to be used for Unit Tests only
    public sealed class SimpleEntitiesSubmissionScheduler : ISimpleEntitiesSubmissionScheduler
    {
        public override void SubmitEntities()
        {
            if (paused == false)
                _onTick.Invoke();
        }

        protected internal override EnginesRoot.EntitiesSubmitter onTick
        {
            set
            {
                DBC.ECS.Check.Require(_onTick.IsUnused , "a scheduler can be exclusively used by one enginesRoot only");
                
                _onTick = value;
            }
        }

        public override bool paused { get; set; }

        public override void Dispose() { }

        EnginesRoot.EntitiesSubmitter _onTick;
    }
}