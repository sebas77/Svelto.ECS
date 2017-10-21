namespace Svelto.ECS
{
    public interface INode
    {}

    public interface INodeWithID:INode
    {
        int ID { get; }
    }

    public interface IStructNodeWithID : INode
    {
        short ID { get; set; }
    }

    public interface IGroupedStructNodeWithID : IStructNodeWithID
    {
        short groupID { get; set; }
    }

    public class NodeWithID: INodeWithID
    {
        public static TNodeType BuildNode<TNodeType>(int ID) where TNodeType: NodeWithID, new() 
        {
            return new TNodeType { _ID = ID };
        }

        public int ID { get { return _ID; } }

        protected int _ID;
    }
}
