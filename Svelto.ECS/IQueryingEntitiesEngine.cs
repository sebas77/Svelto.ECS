namespace Svelto.ECS
{
    public interface IQueryingEntitiesEngine : IEngine
    {
        IEntityDB entitiesDB { set; }

        void Ready();
    }
}