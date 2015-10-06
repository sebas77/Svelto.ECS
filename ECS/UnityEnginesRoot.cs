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
#if UNITY_5_0
            INodeHolder[] nodeHolders = entity.GetComponents<INodeHolder>();
            
            for (int i = 0; i < nodeHolders.Length; i++)
                nodeHolders[i].engineRoot = this;
#else
            MonoBehaviour[] nodeHolders = entity.GetComponents<MonoBehaviour>();

            for (int i = 0; i < nodeHolders.Length; i++)
            {
                var nodeHolder = nodeHolders[i];
                
                if (nodeHolder is INodeHolder)
                    (nodeHolders[i] as INodeHolder).engineRoot = this;
            }
#endif
            
        }

        public void RemoveGameObjectEntity(GameObject entity)
        {
#if UNITY_5_0
            INodeHolder[] nodeHolders = entity.GetComponents<INodeHolder>();

            for (int i = 0; i < nodeHolders.Length; i++)
                Remove(nodeHolders[i].node);
#else
            MonoBehaviour[] nodeHolders = entity.GetComponents<MonoBehaviour>();

            for (int i = 0; i < nodeHolders.Length; i++)
            {
                var nodeHolder = nodeHolders[i];
                
                if (nodeHolder is INodeHolder)
                    Remove((nodeHolder as INodeHolder).node);
            }
#endif
        }

        EnginesRoot _engineRoot = new EnginesRoot();
    }
}
