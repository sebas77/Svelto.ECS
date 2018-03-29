using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public abstract class SingleEntityViewEngine<T> : IHandleEntityViewEngine where T : class, IEntityView
    {
        public void Add(IEntityView entityView)
        {
            Add((T) entityView);
        }

        public void Remove(IEntityView entityView)
        {
            Remove((T) entityView);
        }

        protected abstract void Add(T    entityView);
        protected abstract void Remove(T entityView);
    }
}