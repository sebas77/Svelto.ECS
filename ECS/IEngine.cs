namespace Svelto.ES
{
    public interface IEngine
    {}

    public interface INodeEngine<in TNodeType>:IEngine where TNodeType:INode
    {
        void Add(TNodeType obj);
        void Remove(TNodeType obj);
    }

    public interface INodesEngine : INodeEngine<INode>
    {
        System.Type[] AcceptedNodes();
    }

    public interface IQueryableNodeEngine:IEngine
    {
        IEngineNodeDB nodesDB { set; }
    }

    public abstract class SingleNodeEngine<TNodeType> : INodeEngine<INode> where TNodeType:class, INode
    {
        void INodeEngine<INode>.Add(INode obj)
        {
            Add(obj as TNodeType);
        }

        void INodeEngine<INode>.Remove(INode obj)
        {
            Remove(obj as TNodeType);
        }

        protected abstract void Add(TNodeType node);
        protected abstract void Remove(TNodeType node);
    }
}
