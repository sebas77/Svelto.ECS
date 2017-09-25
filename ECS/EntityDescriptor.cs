using System;
using System.Reflection;
using Svelto.DataStructures;
#if NETFX_CORE
using BindingFlags = System.Reflection.BindingFlags;
#endif

namespace Svelto.ECS
{
    public class EntityDescriptor
    {
        protected EntityDescriptor(INodeBuilder[] nodesToBuild, params object[] componentsImplementor)
        {
            _implementors = componentsImplementor; 
            _nodesToBuild = nodesToBuild;
        }

  /*      protected EntityDescriptor(IStructNodeBuilder[] structNodesToBuild)
        {
            _structNodesToBuild = structNodesToBuild;
        }*/

        public void AddImplementors(params object[] componentsImplementor)
        {
            var implementors = new object[componentsImplementor.Length + _implementors.Length];

            Array.Copy(_implementors, implementors, _implementors.Length);
            Array.Copy(componentsImplementor, 0, implementors, _implementors.Length, componentsImplementor.Length);

            _implementors = implementors;
        }

        public virtual FasterList<INode> BuildNodes(int ID, Action<FasterReadOnlyList<INode>> removeAction)
        {
            var nodes = new FasterList<INode>();

            for (int index = _nodesToBuild.Length - 1; index >= 0; index--)
            {
                var nodeBuilder = _nodesToBuild[index];
                var node = FillNode(nodeBuilder.Build(ID), () =>
                    {
                        removeAction(new FasterReadOnlyList<INode>());

                        nodes.Clear();
                    }
                );

                nodes.Add(node);
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

        object[]       _implementors;

        readonly INodeBuilder[]         _nodesToBuild;
   //     readonly IStructNodeBuilder[]   _structNodesToBuild;
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
/*
    public interface IStructNodeBuilder
    {}

    public class StructNodeBuilder<NodeType> : IStructNodeBuilder where NodeType : struct
    {
        public NodeType Build()
        {
            return new NodeType();
        }
    }*/
}
