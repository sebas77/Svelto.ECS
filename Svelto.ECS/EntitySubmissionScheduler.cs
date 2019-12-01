namespace Svelto.ECS.Schedulers
{
    public interface IEntitySubmissionScheduler
    {
        EnginesRoot.EntitiesSubmitter onTick { set; }
    }
}