using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public interface IReactOnAddAndRemove<T> : IReactOnAddAndRemove where T : IEntityComponent
    {
        void Add(ref T entityComponent, EGID egid);
        void Remove(ref T entityComponent, EGID egid);
    }
 }