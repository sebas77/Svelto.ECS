using Svelto.DataStructures;

namespace Svelto.ES
{
    public interface IEngineNodeDB
    {
        ReadOnlyDictionary<int, INode> QueryIndexableNodes<T>() where T : INode;
        bool QueryNode<T>(int ID, out T node) where T:INode;
        FasterReadOnlyList<INode> QueryNodes<T>() where T : INode;
    }
}
