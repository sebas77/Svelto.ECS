namespace Svelto.ECS
{
    public abstract class SingleNodeEngine<TNodeType> : INodeEngine where TNodeType : class, INode
    {
        void INodeEngine.Add(INode obj)
        {
            Add(obj as TNodeType);
        }

        void INodeEngine.Remove(INode obj)
        {
            Remove(obj as TNodeType);
        }

        protected abstract void Add(TNodeType node);
        protected abstract void Remove(TNodeType node);
    }
}
