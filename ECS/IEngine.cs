using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Internal
{
    public interface IStructNodeEngine : IEngine
    {
        void CreateStructNodes(SharedStructNodeLists sharedStructNodeLists);
    }

    public interface IGroupedStructNodesEngine : IEngine
    {
        void CreateStructNodes(SharedGroupedStructNodesLists sharedStructNodeLists);
    }

    public interface IActivableNodeEngine : IEngine
    {
        void Enable(ITypeSafeList nodes);
        void Disable(ITypeSafeList nodes);
    }

    public interface INodeEngine : IEngine
    {
        void Add(ITypeSafeList nodes);
        void Remove(ITypeSafeList nodes);
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

    /// <summary>
    /// The engines can receive and store INodes structs
    /// Unboxing will happen during the Add, but the 
    /// data will then be stored and processed as stucts
    /// </summary>
    public interface IStructNodeEngine<T> : IStructNodeEngine where T:struct, IStructNodeWithID
    { }

    /// <summary>
    /// same as above, but the nodes are grouped by ID
    /// usually the ID is the owner of the nodes of that
    /// group
    /// </summary>
    public interface IGroupedStructNodesEngine<T> : IGroupedStructNodesEngine where T:struct, IGroupedNode
    { }
}

