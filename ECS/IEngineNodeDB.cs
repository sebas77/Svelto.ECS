using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IEngineNodeDB
    {
        ReadOnlyDictionary<int, T> QueryIndexableNodes<T>();
        
        bool TryQueryNode<T>(int ID, out T node);
        T QueryNode<T>(int ID); 
        FasterReadOnlyList<T> QueryNodes<T>();

        bool TryQueryMetaNode<T>(int metaEntityID, out T node);
        T QueryMetaNode<T>(int metaEntityID);
        FasterReadOnlyList<T> QueryMetaNodes<T>();
    }
}

