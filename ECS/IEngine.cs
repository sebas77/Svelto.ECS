using Svelto.ECS.Internal;

namespace Svelto.ECS.Internal
{
    public interface IActivableNodeEngine : IEngine
    {
        void Enable(INode obj);
        void Disable(INode obj);
    }

    public interface INodeEngine : IEngine
    {
        void Add(INode obj);
        void Remove(INode obj);
    }

    public interface INodesEngine : INodeEngine
    {
        System.Type[] AcceptedNodes();
    }
}

namespace Svelto.ECS
{
    public interface IEngine
    {}

    public interface IActivableNodeEngine<in TNodeType> : IActivableNodeEngine where TNodeType : INode
    { }

    public interface IQueryableNodeEngine:IEngine
    {
        IEngineNodeDB nodesDB { set; }
    }
}

