namespace Svelto.ES
{
	public interface IEngine 
	{}
    
    public interface INodeEngine : IEngine
    {
        System.Type[] AcceptedNodes();

        void Add(INode obj);
        void Remove(INode obj);
    }

}
