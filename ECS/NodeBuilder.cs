using System;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public interface INodeBuilder
    {
        INode BuildAndAddToList(ref ITypeSafeList list, int entityID);

        Type GetNodeType();

        FillNodeMode reflects { get; }
    }

    public class NodeBuilder<NodeType> : INodeBuilder where NodeType : NodeWithID, new()
    {
        public INode BuildAndAddToList(ref ITypeSafeList list, int entityID)
        {
            if (list == null)
                list = new TypeSafeFasterList<NodeType>();

            var castedList = list as FasterList<NodeType>;

            var node = NodeWithID.BuildNode<NodeType>(entityID);

            castedList.Add(node);

            return node;
        }

        public FillNodeMode reflects
        {
            get { return FillNodeMode.Strict; }
        }

        public Type GetNodeType()
        {
            return typeof(NodeType);
        }
    }

    public class StructNodeBuilder<NodeType> : INodeBuilder where NodeType : struct, IStructNodeWithID
    {
        public INode BuildAndAddToList(ref ITypeSafeList list, int entityID)
        {
            var node = default(NodeType);
            node.ID = entityID;
            
            if (list == null)
                list = new TypeSafeFasterList<NodeType>();

            var castedList = list as FasterList<NodeType>;

            castedList.Add(node);

            return null;
        }

        public Type GetNodeType()
        {
            return typeof(NodeType);
        }

        public virtual FillNodeMode reflects
        {
            get { return FillNodeMode.None; }
        }
    }
    
    public enum FillNodeMode
    {
        Strict,
    
        None
    }
}