using System;
using System.Reflection;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public class EntityDescriptor
    {
        protected EntityDescriptor(INodeBuilder[] nodesToBuild, params object[] componentsImplementor)
        {
            _implementors = componentsImplementor; 
            _nodesToBuild = nodesToBuild;
        }

        public virtual FasterList<INode> BuildNodes(int ID, Action<INode> removeAction)
        {
            var nodes = new FasterList<INode>();

            for (int index = 0; index < _nodesToBuild.Length; index++)
            {
                var nodeBuilder = _nodesToBuild[index];
                var node = FillNode(nodeBuilder.Build(ID), () =>
                    {
                        for (int i = 0; i < nodes.Count; i++)
                            removeAction(nodes[i]);

                        nodes.Clear();
                    }
                );

                nodes.Add (node);
            }

            return nodes;
        }

        TNode FillNode<TNode>(TNode node, Action removeAction) where TNode: INode
        {
            var fields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            for (int i = fields.Length - 1; i >=0 ; --i)
            {
                var     field       = fields[i];
                Type    fieldType   = field.FieldType;
                object  component   = null;

                for (int j = 0; j < _implementors.Length; j++)
                {
                    var implementor = _implementors[j];

                    if (implementor != null && fieldType.IsAssignableFrom(implementor.GetType()))
                    {
                        component = implementor;

                        if (fieldType.IsAssignableFrom(typeof(IRemoveEntityComponent)))
                            (component as IRemoveEntityComponent).removeEntity = removeAction;

                        break;
                    }
                }

                if (component == null)
                {
                    Exception e = new Exception("Svelto.ECS: Implementor not found for a Node. " +
                                                "Implementor Type: " + field.FieldType.Name + " - Node: " + node.GetType().Name + " - EntityDescriptor " + this);

                    throw e;
                }

                field.SetValue(node, component);
            }

            return node;
        }

        readonly object[]   _implementors;
        INodeBuilder[]      _nodesToBuild;
    }

    public interface INodeBuilder
    {
        NodeWithID Build(int ID);
    }

    public class NodeBuilder<NodeType> : INodeBuilder where NodeType:NodeWithID, new()
    {
        public NodeWithID Build(int ID)
        {
            NodeWithID node = NodeWithID.BuildNode<NodeType>(ID);

            return (NodeType)node;
        }
    }
}
