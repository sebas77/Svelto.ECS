namespace Svelto.ECS
{
    public abstract class MultiEntityViewsEngine<T, U> : SingleEntityViewEngine<T>, IHandleEntityStructEngine<U>
        where U : IEntityData where T : IEntityData
    {
        public abstract void Add(ref    U entityView);
        public abstract void Remove(ref U entityView);
    }

    public abstract class MultiEntityViewsEngine<T, U, V> : MultiEntityViewsEngine<T, U>, IHandleEntityStructEngine<V>
        where V :  IEntityData where U :  IEntityData where T :  IEntityData
    {
        public abstract void Add(ref    V entityView);
        public abstract void Remove(ref V entityView);
    }

    /// <summary>
    ///     Please do not add more MultiEntityViewsEngine
    ///     if you use more than 4 nodes, your engine has
    ///     already too many responsabilities.
    /// </summary>
    public abstract class MultiEntityViewsEngine<T, U, V, W> : MultiEntityViewsEngine<T, U, V>, IHandleEntityStructEngine<W>
        where W :  IEntityData where V :  IEntityData where U :  IEntityData where T : IEntityData
    {
        public abstract void Add(ref    W entityView);
        public abstract void Remove(ref W entityView);
    }
}