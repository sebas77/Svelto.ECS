using Svelto.ECS.Internal;

namespace Svelto.ECS.Internal
{
    public abstract class MultiEntityViewsEngine<T> : IHandleEntityViewEngine where T : class, IEntityView
    {
        public virtual void Add(IEntityView entityView)
        {
            Add((T) entityView);
        }

        public virtual void Remove(IEntityView entityView)
        {
            Remove((T) entityView);
        }

        protected abstract void Add(T    entityView);
        protected abstract void Remove(T entityView);
    }
}

namespace Svelto.ECS
{
    public abstract class MultiEntityViewsEngine<T, U> : MultiEntityViewsEngine<T>
        where U : class, IEntityView where T : class, IEntityView
    {
        protected abstract void Add(U    entityView);
        protected abstract void Remove(U entityView);

        public override void Add(IEntityView entityView)
        {
            if (entityView is U)
                Add((U) entityView);
            else
                base.Add(entityView);
        }

        public override void Remove(IEntityView entityView)
        {
            if (entityView is U)
                Remove((U) entityView);
            else
                base.Remove(entityView);
        }
    }

    public abstract class MultiEntityViewsEngine<T, U, V> : MultiEntityViewsEngine<T, U>
        where V :  class, IEntityView where U :  class, IEntityView where T :  class, IEntityView
    {
        protected abstract void Add(V    entityView);
        protected abstract void Remove(V entityView);

        public override void Add(IEntityView entityView)
        {
            if (entityView is V)
                Add((V) entityView);
            else
                base.Add(entityView);
        }

        public override void Remove(IEntityView entityView)
        {
            if (entityView is V)
                Remove((V) entityView);
            else
                base.Remove(entityView);
        }
    }

    /// <summary>
    ///     Please do not add more MultiEntityViewsEngine
    ///     if you use more than 4 nodes, your engine has
    ///     already too many responsabilities.
    /// </summary>
    public abstract class MultiEntityViewsEngine<T, U, V, W> : MultiEntityViewsEngine<T, U, V>
        where W :  class, IEntityView where V :  class, IEntityView where U :  class, IEntityView where T : class, IEntityView
    {
        protected abstract void Add(W    entityView);
        protected abstract void Remove(W entityView);

        public override void Add(IEntityView entityView)
        {
            if (entityView is W)
                Add((W) entityView);
            else
                base.Add(entityView);
        }

        public override void Remove(IEntityView entityView)
        {
            if (entityView is W)
                Remove((W) entityView);
            else
                base.Remove(entityView);
        }
    }
}