using Svelto.ECS.Internal;

namespace Svelto.ECS.Internal
{
    public interface IHandleEntityViewEngineAbstracted : IEngine
    {}
}

namespace Svelto.ECS
{
    public interface IEngine
    {}
    
    public interface IHandleEntityStructEngine<T> : IHandleEntityViewEngineAbstracted
    {
        void Add(ref T entityView);
        void Remove(ref T entityView);
    }
}