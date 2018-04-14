using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public abstract class SingleEntityViewEngine<T> : IHandleEntityViewEngine where T : class, IEntityData
    {
        public void Add(IEntityData entityView)
        {
            Add((T) entityView);
        }

        public void Remove(IEntityData entityView)
        {
            Remove((T) entityView);
        }

        protected abstract void Add(T    entityView);
        protected abstract void Remove(T entityView);
    }
}