namespace Svelto.ES
{
    public interface INode
    {}

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
