using Svelto.ECS.Internal;

namespace Svelto.ECS.Internal
{
    public abstract class MultiNodesEngine<T>
        where T : INode
    {
        protected abstract void AddNode(T node);
        protected abstract void RemoveNode(T node);
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

    public abstract class MultiNodesEngine<T, U> : MultiNodesEngine<T>, 
        INodeEngine 
        where T : INode
        where U : INode
    {
        protected abstract void AddNode(U node);
        protected abstract void RemoveNode(U node);

        public void Add(INode node)
        {
            if (node is T)
                AddNode((T)node);
            else
                AddNode((U)node);
        }

        public void Remove(INode node)
        {
            if (node is T)
                RemoveNode((T)node);
            else
                RemoveNode((U)node);
        }
    }
}