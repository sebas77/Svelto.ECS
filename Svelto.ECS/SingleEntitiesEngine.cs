namespace Svelto.ECS
{
    public abstract class SingleEntitiesEngine<T> : IHandleEntityStructEngine<T> where T : struct, IEntityData
    {
        public void AddInternal(ref T entityView)
        { Add(ref entityView); }

        public void RemoveInternal(ref T entityView)
        { Remove(ref entityView); }

        protected abstract void Add(ref    T entityView);
        protected abstract void Remove(ref T entityView);
    }
}