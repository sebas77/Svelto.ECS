namespace Svelto.ECS
{
    public abstract class SingleEntityViewEngine<T> : IHandleEntityStructEngine<T> where T : IEntityData
    {
        public void AddInternal(ref T entityView)
        { Add(entityView); }

        public void RemoveInternal(ref T entityView)
        { Remove(entityView); }

        protected abstract void Add(T entityView);
        protected abstract void Remove(T entityView);
    }
    
    public abstract class SingleEntityStructEngine<T> : IHandleEntityStructEngine<T> where T : IEntityData
    {
        public void AddInternal(ref T entityView)
        { Add(ref entityView); }

        public void RemoveInternal(ref T entityView)
        { Remove(ref entityView); }

        protected abstract void Add(ref T    entityView);
        protected abstract void Remove(ref T entityView);
    }
}