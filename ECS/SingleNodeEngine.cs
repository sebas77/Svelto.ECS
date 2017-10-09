using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public abstract class SingleNodeEngine<TNodeType> : INodeEngine 
        where TNodeType : class, INode
    {
        public void Add(INode obj)
        {
            Add((TNodeType) obj);
        }

        public void Remove(INode obj)
        {
            Remove((TNodeType) obj);
        }

        protected abstract void Add(TNodeType node);
        protected abstract void Remove(TNodeType node);
    }
}
