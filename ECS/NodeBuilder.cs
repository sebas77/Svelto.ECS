using System;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public interface INodeBuilder
    {
        INode BuildNodeAndAddToList(ref ITypeSafeList list, int entityID);

        Type GetNodeType();
    }

    public class NodeBuilder<NodeType> : INodeBuilder where NodeType : NodeWithID, new()
    {
        public INode BuildNodeAndAddToList(ref ITypeSafeList list, int entityID)
        {
            if (list == null)
                list = new TypeSafeFasterListForECSForClasses<NodeType>();

            var castedList = list as TypeSafeFasterListForECSForClasses<NodeType>;

            var node = NodeWithID.BuildNode<NodeType>(entityID);

            castedList.Add(node);

            return node;
        }

        public Type GetNodeType()
        {
            return typeof(NodeType);
        }
    }

    public class StructNodeBuilder<NodeType> : INodeBuilder where NodeType : struct, IStructNodeWithID
    {
        public INode BuildNodeAndAddToList(ref ITypeSafeList list, int entityID)
        {
            var node = default(NodeType);
            node.ID = entityID;
            
            if (list == null)
                list = new TypeSafeFasterListForECSForStructs<NodeType>();

            var castedList = list as TypeSafeFasterListForECSForStructs<NodeType>;

            castedList.Add(node);

            return null;
        }

        public Type GetNodeType()
        {
            return typeof(NodeType);
        }
    }
}