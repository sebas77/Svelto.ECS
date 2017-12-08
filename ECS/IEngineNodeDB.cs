using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IEngineNodeDB
    {
        FasterReadOnlyList<T> QueryNodes<T>();
        FasterReadOnlyList<T> QueryMetaNodes<T>();
        FasterReadOnlyList<T> QueryGroupedNodes<T>(int group);
        
        T[] QueryNodesAsArray<T>(out int count) where T:struct;
        
        ReadOnlyDictionary<int, T> QueryIndexableNodes<T>()  where T:NodeWithID;
        
        bool TryQueryNode<T>(int ID, out T node) where T:NodeWithID;
        T QueryNode<T>(int ID) where T:NodeWithID; 

        bool TryQueryMetaNode<T>(int metaEntityID, out T node)  where T:NodeWithID;
        T QueryMetaNode<T>(int metaEntityID) where T:NodeWithID;
    }
}

