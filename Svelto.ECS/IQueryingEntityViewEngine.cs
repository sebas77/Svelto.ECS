namespace Svelto.ECS
{
    public interface IQueryingEntityViewEngine : IEngine
    {
        IEntityViewsDB entityViewsDB { set; }

        void Ready();
    }
}