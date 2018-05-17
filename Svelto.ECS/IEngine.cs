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
        void AddInternal(ref T entityView);
        void RemoveInternal(ref T entityView);
    }
}