using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public interface IReactOnAddAndRemove<T> : IReactOnAddAndRemove where T : IEntityStruct
    {
        void Add(ref T entityView);
        void Remove(ref T entityView);
    }
 }