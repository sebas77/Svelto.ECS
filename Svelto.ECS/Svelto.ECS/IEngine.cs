namespace Svelto.ECS.Internal
{
    public interface IReactEngine: IEngine
    {}
    
    public interface IReactOnAddAndRemove : IReactEngine
    {}

    public interface IReactOnSwap : IReactEngine
    {}
}

namespace Svelto.ECS
{
    public interface IEngine
    {}
}