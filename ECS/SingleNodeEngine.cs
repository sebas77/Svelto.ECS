namespace Svelto.ECS
{
    public abstract class SingleNodeEngine<TNodeType> : INodeEngine<INode> where TNodeType : class, INode
    {
        void INodeEngine<INode>.Add(INode obj)
        {
            Add(obj as TNodeType);
        }

        void INodeEngine<INode>.Remove(INode obj)
        {
            Remove(obj as TNodeType);
        }

        protected virtual void Add(TNodeType node)
        {}
        protected virtual void Remove(TNodeType node)
        {}
    }
}
