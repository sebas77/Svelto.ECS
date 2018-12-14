using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public abstract class SingleEntityViewEngine<T> : EngineInfo, IHandleEntityStructEngine<T> where T : class, IEntityStruct
    {
        public void AddInternal(ref T entityView)
        { Add(entityView); }

        public void RemoveInternal(ref T entityView)
        { Remove(entityView); }

        protected abstract void Add(T entityView);
        protected abstract void Remove(T entityView);
    }
}