using Svelto.ECS.Internal;

namespace Svelto.ECS.Internal
{
    public interface IHandleEntityViewEngineAbstracted : IEngine
    {}
    
    public interface IHandleEntityViewEngine : IHandleEntityViewEngineAbstracted
    {
        void Remove(IEntityView entityView);
    }
}

namespace Svelto.ECS
{
    public interface IEngine
    {}
    
    public interface IHandleEntityStructEngine<T> : IHandleEntityViewEngineAbstracted
    {
        void Add(ref T entityView);
    }
}