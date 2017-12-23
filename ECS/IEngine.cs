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
#if EXPERIMENTAL
    public interface IHandleActivableEntityEngine : IEngine
    {
        void Enable(EntityView entityView);
        void Disable(EntityView entityView);
    }
#endif
    public interface IQueryingEntityViewEngine : IEngine
    {
        IEngineEntityViewDB entityViewsDB { set; }

        void Ready();
    }
}
