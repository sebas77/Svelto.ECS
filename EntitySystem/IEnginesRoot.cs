namespace Svelto.ES
{
    public interface IEnginesRoot
    {
        void AddEngine(IEngine engine);
    }

    public interface INodeEnginesRoot: IEnginesRoot
    {
        void Add(INode node);
        void Remove(INode node);
    }
}
