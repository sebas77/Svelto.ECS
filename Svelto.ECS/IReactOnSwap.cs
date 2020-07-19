using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public interface IReactOnSwap<T> : IReactOnSwap where T : IEntityComponent
    {
        void MovedTo(ref T entityComponent, ExclusiveGroupStruct previousGroup, EGID egid);
    }
}