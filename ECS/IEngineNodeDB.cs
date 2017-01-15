using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IEngineNodeDB
    {
        ReadOnlyDictionary<int, INode> QueryIndexableNodes<T>() where T:INode;
        
        bool QueryNode<T>(int ID, out T node) where T:INode;
        T QueryNode<T>(int ID) where T:INode;
        
        FasterReadOnlyListCast<INode, T> QueryNodes<T>() where T:INode;

        bool QueryNodeFromGroup<T>(int ID, out T node) where T : INode;
        T QueryNodeFromGroup<T>(int ID) where T : INode;
        FasterReadOnlyListCast<INode, T> QueryNodesFromGroups<T>() where T : INode;
    }
}

