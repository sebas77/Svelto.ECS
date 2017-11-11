using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Internal
{
    public abstract class MultiNodesEngine<T> where T:class
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

        public abstract void Add(ITypeSafeList nodeWrapper);
        public abstract void Remove(ITypeSafeList nodeWrapper);
    }

    public abstract class MultiNodesEngine<T, U> : MultiNodesEngine<T>, 
        INodeEngine where T:class where U:class
    {
        protected abstract void AddNode(U node);
        protected abstract void RemoveNode(U node);

        public virtual void Add(ITypeSafeList nodes)
        {
            if (nodes is FasterList<U>)
            {
                var strongTypeNodes = (FasterList<U>)nodes;

                for (int i = 0; i < strongTypeNodes.Count; i++)
                {
                    AddNode(strongTypeNodes[i]);
                }
            }
            else
            if (nodes is FasterList<T>)
            {
                var strongTypeNodes = (FasterList<T>)nodes;

                for (int i = 0; i < strongTypeNodes.Count; i++)
                {
                    AddNode(strongTypeNodes[i]);
                }
            }
        }

        public virtual void Remove(ITypeSafeList nodeWrapper)
        {
        /*    if (nodeWrapper is NodeWrapper<T>)
            {
                T node;
                nodeWrapper.GetNode<T>(out node);

                RemoveNode(ref node);
            }
            else
            {
                U node;
                nodeWrapper.GetNode<U>(out node);

                RemoveNode(ref node);
            }*/
        }
    }

    public abstract class MultiNodesEngine<T, U, V> : MultiNodesEngine<T, U> where T: class where U : class
    {
        protected abstract void AddNode(V node);
        protected abstract void RemoveNode(V node);

        public override void Add(ITypeSafeList nodes)
        {
            if (nodes is FasterList<V>)
            {
                var strongTypeNodes = (FasterList<V>)nodes;

                for (int i = 0; i < strongTypeNodes.Count; i++)
                {
                    AddNode(strongTypeNodes[i]); 
                }
            }
            else
                base.Add(nodes);
        }

        public override void Remove(ITypeSafeList nodeWrapper)
        {
          /*  if (nodeWrapper is V)
            {
                V node;
                nodeWrapper.GetNode<V>(out node);

                RemoveNode(ref node);
            }
            else
                base.Remove(nodeWrapper);*/
        }
    }
}