using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public abstract class SingleNodeEngine<T> : INodeEngine where T:NodeWithID
    {
        public void Add(NodeWithID node)
        {
            Add((T)node); //when byref returns will be vailable, this should be passed by reference, not copy!
        }

        public void Remove(NodeWithID node)
        {
            Remove((T)node);
        }

        protected abstract void Add(T node);
        protected abstract void Remove(T node);
    }
}
