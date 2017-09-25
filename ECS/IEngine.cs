namespace Svelto.ECS
{
    public interface IEngine
    {}

    public interface INodeEngine<in TNodeType>:IEngine where TNodeType:INode
    {
        void Add(TNodeType obj);
        void Remove(TNodeType obj);
    }

    public interface INodeEngine<in T, in U>:INodeEngine<U> where T:INode where U:INode
    {
        void Add(T obj);
        void Remove(T obj);
    }

    public interface INodeEngine<in T, in U, in V>:INodeEngine<U, V> where T:INode where U:INode where V:INode
    {
        void Add(T obj);
        void Remove(T obj);
    }

    public interface INodeEngine:IEngine
    {
        void Add(INode obj);
        void Remove(INode obj);
    }

    public interface INodesEngine : INodeEngine
    {
        System.Type[] AcceptedNodes();
    }

    public interface IQueryableNodeEngine:IEngine
    {
        IEngineNodeDB nodesDB { set; }
    }
}
