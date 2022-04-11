using Svelto.ECS.Schedulers;

namespace Svelto.ECS
{
    public class SerializingEnginesRoot : EnginesRoot
    {
        public SerializingEnginesRoot
            (EntitiesSubmissionScheduler entitiesComponentScheduler) : base(entitiesComponentScheduler)
        { }

        public SerializingEnginesRoot
            (EntitiesSubmissionScheduler entitiesComponentScheduler, EnginesReadyOption enginesWaitForReady) : base(
            entitiesComponentScheduler, enginesWaitForReady)
        {}
    }
}