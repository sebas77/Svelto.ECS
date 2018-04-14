namespace Svelto.ECS.Internal
{
    public interface IHandleEntityViewEngine : IEngine
    {
        void Add(IEntityData    entityView);
        void Remove(IEntityData entityView);
    }
}

namespace Svelto.ECS
{
    public interface IEngine
    {
    }
}