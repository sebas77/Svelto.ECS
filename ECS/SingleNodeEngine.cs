using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public abstract class SingleNodeEngine<T> : INodeEngine where T:class
    {
        public void Add(ITypeSafeList nodes)
        {
            var strongTypeNodes = (FasterList<T>)nodes;

            for (int i = 0; i < strongTypeNodes.Count; i++)
            {
                Add(strongTypeNodes[i]); //when byref returns will be vailable, this should be passed by reference, not copy!
            }
        }

        public void Remove(ITypeSafeList nodes)
        {
            /*
            T node;
            
            nodeWrapper.GetNode(out node);
            
            Remove(node);*/
        }

        protected abstract void Add(T node);
        protected abstract void Remove(T node);
    }
}
