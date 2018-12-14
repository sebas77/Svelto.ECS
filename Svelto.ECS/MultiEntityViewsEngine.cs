using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public abstract class MultiEntityViewsEngine<T, U> : SingleEntityViewEngine<T>, IHandleEntityStructEngine<U>
        where U : class, IEntityStruct where T : class, IEntityStruct
    {
        public void AddInternal(ref    U entityView)
        { Add(entityView); }
        public void RemoveInternal(ref U entityView)
        { Remove(entityView); }
        
        protected abstract void Add(U    entityView);
        protected abstract void Remove(U entityView);
    }

    public abstract class MultiEntityViewsEngine<T, U, V> : MultiEntityViewsEngine<T, U>, IHandleEntityStructEngine<V>
        where V :  class, IEntityStruct where U :  class, IEntityStruct where T :  class, IEntityStruct
    {
        public void AddInternal(ref    V entityView)
        { Add(entityView); }
        public void RemoveInternal(ref V entityView)
        { Remove(entityView); }
        
        protected abstract void Add(V    entityView);
        protected abstract void Remove(V entityView);
    }

    /// <summary>
    ///     Please do not add more MultiEntityViewsEngine
    ///     if you use more than 4 nodes, your engine has
    ///     already too many responsabilities.
    /// </summary>
    public abstract class MultiEntityViewsEngine<T, U, V, W> : MultiEntityViewsEngine<T, U, V>, IHandleEntityStructEngine<W>
        where W :  class, IEntityStruct where V : class, IEntityStruct where U :  class, IEntityStruct where T : class, IEntityStruct
    {
        public void AddInternal(ref    W entityView)
        { Add(entityView); }
        public void RemoveInternal(ref W entityView)
        { Remove(entityView); }
        
        protected abstract void Add(W    entityView);
        protected abstract void Remove(W entityView);
    }
}