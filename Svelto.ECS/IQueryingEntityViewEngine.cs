namespace Svelto.ECS
{
    public interface IQueryingEntityViewEngine : IEngine
    {
        IEntityDB EntityDb { set; }

        void Ready();
    }
}