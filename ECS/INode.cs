namespace Svelto.ECS
{
    public interface INode
    {}

    public interface IStructNodeWithID : INode
    {
        int ID { get; set; }
    }

    public interface IGroupedNode
    {
        int groupID { get; set; }
    }

    public class NodeWithID: INode
    {
        public static TNodeType BuildNode<TNodeType>(int ID) where TNodeType: NodeWithID, new() 
        {
            return new TNodeType { _ID = ID };
        }

        public int ID { get { return _ID; } }

        protected int _ID;
    }
}
