namespace Svelto.ECS
{
    public interface INode
    {
        int ID { get; }
    }
    
    public interface IGroupedNode
    {
        int groupID { get; set; }
    }
    
    public interface IStructNodeWithID : INode
    {
        new int ID { get; set; }
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
