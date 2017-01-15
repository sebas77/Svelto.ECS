namespace Svelto.ECS
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
}
