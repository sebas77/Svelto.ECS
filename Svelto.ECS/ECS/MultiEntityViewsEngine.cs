using Svelto.ECS.Internal;

namespace Svelto.ECS.Internal
{
    public abstract class MultiEntityViewsEngine<T>:IHandleEntityViewEngine where T:EntityView, new()
    {
        protected abstract void Add(T entityView);
        protected abstract void Remove(T entityView);
        
        public virtual void Add(IEntityView entityView)
        {
            Add((T) entityView);
        }

        public virtual void Remove(IEntityView entityView)
        {
            Remove((T) entityView);
        }
    }
}

namespace Svelto.ECS
{
    public abstract class MultiEntityViewsEngine<T, U> : MultiEntityViewsEngine<T>
        where U:EntityView, new() where T : EntityView, new()
    {
        protected abstract void Add(U entityView);
        protected abstract void Remove(U entityView);

        public override void Add(IEntityView entityView)
        {
            var castedEntityView = entityView as U;
            if (castedEntityView != null)
            {
                Add(castedEntityView);
            }
            else
            {
                base.Add(entityView);
            }
        }

        public override void Remove(IEntityView entityView)
        {
            if (entityView is U)
            {
                Remove((U) entityView);
            }
            else
            {
                base.Remove(entityView);
            }
        }
    }

    public abstract class MultiEntityViewsEngine<T, U, V> : MultiEntityViewsEngine<T, U>
        where V : EntityView, new() where U : EntityView, new() where T : EntityView, new()
    {
        protected abstract void Add(V entityView);
        protected abstract void Remove(V entityView);

        public override void Add(IEntityView entityView)
        {
            var castedEntityView = entityView as V;
            if (castedEntityView != null)
            {
                Add(castedEntityView);
            }
            else
                base.Add(entityView);
        }

        public override void Remove(IEntityView entityView)
        {
            var castedEntityView = entityView as V;
            if (castedEntityView != null)
            {
                Remove(castedEntityView);
            }
            else
                base.Remove(entityView);
        }
    }
    
    /// <summary>
    /// Please do not add more MultiEntityViewsEngine
    /// if you use more than 4 nodes, your engine has
    /// already too many responsabilities.
    /// </summary>
    public abstract class MultiEntityViewsEngine<T, U, V, W> : MultiEntityViewsEngine<T, U, V>
        where W : EntityView, new() where V : EntityView, new() where U : EntityView, new() where T : EntityView, new()
    {
        protected abstract void Add(W entityView);
        protected abstract void Remove(W entityView);

        public override void Add(IEntityView entityView)
        {
            var castedEntityView = entityView as W;
            if (castedEntityView != null)
            {
                Add(castedEntityView);
            }
            else
                base.Add(entityView);
        }

        public override void Remove(IEntityView entityView)
        {
            var castedEntityView = entityView as W;
            if (castedEntityView != null)
            {
                Remove(castedEntityView);
            }
            else
                base.Remove(entityView);
        }
    }
}