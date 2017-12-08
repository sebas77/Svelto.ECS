using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Internal
{
    public abstract class MultiNodesEngine<T>:INodeEngine where T:NodeWithID
    {
        protected abstract void Add(T node);
        protected abstract void Remove(T node);
        
        public virtual void Add(NodeWithID node)
        {
            Add((T) node);
        }

        public virtual void Remove(NodeWithID node)
        {
            Remove((T) node);
        }
    }
}

namespace Svelto.ECS
{
    public abstract class MultiNodesEngine<T, U> : MultiNodesEngine<T>
        where T:NodeWithID where U:NodeWithID
    {
        protected abstract void Add(U node);
        protected abstract void Remove(U node);

        public override void Add(NodeWithID node)
        {
            var castedNode = node as U;
            if (castedNode != null)
            {
                Add(castedNode);
            }
            else
            {
                base.Add(node);
            }
        }

        public override void Remove(NodeWithID node)
        {
            if (node is U)
            {
                Remove((U) node);
            }
            else
            {
                base.Remove(node);
            }
        }
    }

    public abstract class MultiNodesEngine<T, U, V> : MultiNodesEngine<T, U> 
        where T: NodeWithID where U : NodeWithID where V:NodeWithID
    {
        protected abstract void Add(V node);
        protected abstract void Remove(V node);

        public override void Add(NodeWithID node)
        {
            var castedNode = node as V;
            if (castedNode != null)
            {
                Add(castedNode);
            }
            else
                base.Add(node);
        }

        public override void Remove(NodeWithID node)
        {
            var castedNode = node as V;
            if (castedNode != null)
            {
                Remove(castedNode);
            }
            else
                base.Remove(node);
        }
    }
}