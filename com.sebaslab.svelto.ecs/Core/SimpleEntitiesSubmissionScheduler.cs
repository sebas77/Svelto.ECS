namespace Svelto.ECS.Schedulers
{
    public sealed class SimpleEntitiesSubmissionScheduler : EntitiesSubmissionScheduler
    {
        protected internal override EnginesRoot.EntitiesSubmitter onTick
        {
            set
            {
                DBC.ECS.Check.Require(_entitiesSubmitter == null, "a scheduler can be exclusively used by one enginesRoot only");

                _entitiesSubmitter = value;
            }
        }

        public override void Dispose() { }

        public void SubmitEntities()
        {
            try
            {
                _entitiesSubmitter.Value.SubmitEntities();
            }
            catch
            {
                paused = true;
                
                throw;
            }
        }

        EnginesRoot.EntitiesSubmitter? _entitiesSubmitter;
    }
}