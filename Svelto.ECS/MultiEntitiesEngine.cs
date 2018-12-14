using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public abstract class MultiEntitiesEngine<T, U> : SingleEntityEngine<T>, IHandleEntityStructEngine<U>
        where U : IEntityStruct where T : IEntityStruct
    {
        public void AddInternal(ref U entityView)
        { Add(ref entityView); }
        public void RemoveInternal(ref U entityView)
        { Remove(ref entityView); }
        
        protected abstract void Add(ref U    entityView);
        protected abstract void Remove(ref U entityView);
    }
    
    public abstract class MultiEntitiesEngine<T, U, V> : MultiEntitiesEngine<T, U>, IHandleEntityStructEngine<V>
        where V :  IEntityStruct where U :  IEntityStruct where T :  IEntityStruct
    {
        public void AddInternal(ref V entityView)
        { Add(ref entityView); }
        public void RemoveInternal(ref V entityView)
        { Remove(ref entityView); }
        
        protected abstract void Add(ref V    entityView);
        protected abstract void Remove(ref V entityView);
    }

    /// <summary>
    ///     Please do not add more MultiEntityViewsEngine if you use more than 4 nodes, your engine has
    ///     already too many responsibilities.
    /// </summary>
    public abstract class MultiEntitiesEngine<T, U, V, W> : MultiEntitiesEngine<T, U, V>, IHandleEntityStructEngine<W>
        where W :  IEntityStruct where V :  IEntityStruct where U :  IEntityStruct where T : IEntityStruct
    {
        public void AddInternal(ref W entityView)
        { Add(ref entityView); }
        public void RemoveInternal(ref W entityView)
        { Remove(ref entityView); }
        
        protected abstract void Add(ref W    entityView);
        protected abstract void Remove(ref W entityView);
    }
}