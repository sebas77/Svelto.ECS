using UnityEngine;

namespace Svelto.ES
{
    public class UnityEnginesRoot : INodeEnginesRoot
    {
        public void AddEngine(IEngine engine)
        {
            _engineRoot.AddEngine(engine);
        }

        public void Add(INode node)
        {
            _engineRoot.Add(node);
        }

        public void Remove(INode node)
        {
            _engineRoot.Remove(node);
        }

        public void AddGameObjectEntity(GameObject entity)
        {
            INodeHolder[] nodeHolders = entity.GetComponents<INodeHolder>();

            for (int i = 0; i < nodeHolders.Length; i++)
                nodeHolders[i].engineRoot = this;
        }

        public void RemoveGameObjectEntity(GameObject entity)
        {
            INodeHolder[] nodeHolders = entity.GetComponents<INodeHolder>();

            for (int i = 0; i < nodeHolders.Length; i++)
                Remove(nodeHolders[i].node);
        }

        EnginesRoot _engineRoot = new EnginesRoot();
    }
}
