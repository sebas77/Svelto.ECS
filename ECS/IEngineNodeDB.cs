using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IEngineNodeDB
    {
        ReadOnlyDictionary<int, INode> QueryIndexableNodes<T>() where T:INode;
        
        bool TryQueryNode<T>(int ID, out T node) where T:INode;
        T QueryNode<T>(int ID) where T:INode;
        
        FasterReadOnlyListCast<INode, T> QueryNodes<T>() where T:INode;

        bool TryQueryMetaNode<T>(int metaEntityID, out T node) where T : INode;
        T QueryMetaNode<T>(int metaEntityID) where T : INode;
        FasterReadOnlyListCast<INode, T> QueryMetaNodes<T>() where T : INode;
    }
}

