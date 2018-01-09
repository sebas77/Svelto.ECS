namespace Svelto.ECS.Internal
{
    public interface IHandleEntityViewEngine : IEngine
    {
        void Add(IEntityView entityView);
        void Remove(IEntityView entityView);
    }
}

namespace Svelto.ECS
{
    public interface IEngine
    {}
    
    public interface IQueryingEntityViewEngine : IEngine
    {
        IEntityViewsDB entityViewsDB { set; }

        void Ready();
    }
}
