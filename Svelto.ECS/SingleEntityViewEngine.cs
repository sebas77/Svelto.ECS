namespace Svelto.ECS
{
    public abstract class SingleEntityViewEngine<T> : IHandleEntityStructEngine<T> where T : IEntityData
    {
        public abstract void Add(ref T    entityView);
        public abstract void Remove(ref T entityView);
    }
}