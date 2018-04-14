using Svelto.ECS.Internal;

namespace Svelto.ECS.Internal
{
    public abstract class MultiEntityViewsEngine<T>:IHandleEntityStructEngine<T>, 
                                                    IHandleEntityViewEngine where T:IEntityData
    {
        public void Add(ref T entityView)
        {
            Add(entityView);
        }

        public virtual void Remove(IEntityData entityView)
        {
            Remove((T) entityView);
        }

        protected abstract void Add(T entityView);
        protected abstract void Remove(T entityView);
    }
}

namespace Svelto.ECS
{
    public abstract class MultiEntityViewsEngine<T, U> : MultiEntityViewsEngine<T>, IHandleEntityStructEngine<U>
        where U : IEntityData where T : IEntityData
    {
        protected abstract void Add(U    entityView);
        protected abstract void Remove(U entityView);

        public override void Remove(IEntityData entityView)
        {
            if (entityView is U)
                Remove((U) entityView);
            else
                base.Remove(entityView);
        }

        public void Add(ref U entityView)
        {
            Add(entityView);
        }
    }

    public abstract class MultiEntityViewsEngine<T, U, V> : MultiEntityViewsEngine<T, U>, IHandleEntityStructEngine<V>
        where V :  IEntityData where U :  IEntityData where T :  IEntityData
    {
        protected abstract void Add(V    entityView);
        protected abstract void Remove(V entityView);

        public override void Remove(IEntityData entityView)
        {
            if (entityView is V)
                Remove((V) entityView);
            else
                base.Remove(entityView);
        }

        public void Add(ref V entityView)
        {
            Add(entityView);
        }
    }

    /// <summary>
    ///     Please do not add more MultiEntityViewsEngine
    ///     if you use more than 4 nodes, your engine has
    ///     already too many responsabilities.
    /// </summary>
    public abstract class MultiEntityViewsEngine<T, U, V, W> : MultiEntityViewsEngine<T, U, V>, IHandleEntityStructEngine<W>
        where W :  IEntityData where V :  IEntityData where U :  IEntityData where T : IEntityData
    {
        protected abstract void Add(W    entityView);
        protected abstract void Remove(W entityView);

        public override void Remove(IEntityData entityView)
        {
            if (entityView is W)
                Remove((W) entityView);
            else
                base.Remove(entityView);
        }

        public void Add(ref W entityView)
        {
            Add(entityView);
        }
    }
}