namespace Svelto.ECS
{
    public interface IQueryingEntitiesEngine : IEngine
    {
        IEntitiesDB entitiesDB { set; }

        void Ready();
    }
}