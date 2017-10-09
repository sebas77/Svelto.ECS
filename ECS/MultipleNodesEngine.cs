using Svelto.ECS.Internal;

namespace Svelto.ECS.Internal
{
    public abstract class MultiNodesEngine<T>
        where T : INode
    {
        protected internal abstract void Add(T node);
        protected internal abstract void Remove(T node);
    }
}

namespace Svelto.ECS
{
    public abstract class MultiNodesEngine : INodesEngine
    {
        public abstract System.Type[] AcceptedNodes();

        public abstract void Add(INode node);
        public abstract void Remove(INode node);
    }

    public abstract class MultiNodesEngine<T, U> : MultiNodesEngine<U>, 
        INodeEngine 
        where T : INode 
        where U : INode
    {
        protected abstract void Add(T node);
        protected abstract void Remove(T node);

        public void Add(INode node)
        {
            if (node is T)
                Add((T) node);
            else
                ((MultiNodesEngine<U>)(this)).Add((U)node);
        }

        public void Remove(INode node)
        {
            if (node is T)
                Remove((T)node);
            else
                ((MultiNodesEngine<U>)(this)).Remove((U)node);
        }
    }
}