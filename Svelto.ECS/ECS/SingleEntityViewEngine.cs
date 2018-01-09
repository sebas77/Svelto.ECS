using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public abstract class SingleEntityViewEngine<T> : IHandleEntityViewEngine where T:EntityView, new()
    {
        public void Add(IEntityView entityView)
        {
            Add((T)entityView); //when byref returns will be vailable, this should be passed by reference, not copy!
        }

        public void Remove(IEntityView entityView)
        {
            Remove((T)entityView);
        }

        protected abstract void Add(T entityView);
        protected abstract void Remove(T entityView);
    }
}
