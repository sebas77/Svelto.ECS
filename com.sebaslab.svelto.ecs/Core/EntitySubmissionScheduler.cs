namespace Svelto.ECS.Schedulers
{
    public class EntitiesSubmissionScheduler
    {
        public bool paused    { get; set; }
        public uint iteration { get; protected internal set; }

        internal bool isRunning;
        
        protected internal EnginesRoot.EntitiesSubmitter onTick
        {
            set
            {
                DBC.ECS.Check.Require(_entitiesSubmitter == null, "a scheduler can be exclusively used by one enginesRoot only");

                _entitiesSubmitter = value;
            }
        }

        public void Dispose() { }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
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