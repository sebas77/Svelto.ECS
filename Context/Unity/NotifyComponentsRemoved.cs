using Svelto.DataStructures;
using UnityEngine;

namespace Svelto.Context.Legacy
{
    public class NotifyComponentsRemoved : MonoBehaviour
    {
        public WeakReference<IUnityContextHierarchyChangedListener> unityContext { private get; set; }

        void Start()
        {
            if (unityContext == null)
            {
                Destroy(this);
            }
        }

        void OnDestroy()
        {
            if (unityContext == null || unityContext.IsAlive == false)
                return;

            MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();

            for (int i = 0; i < components.Length; ++i)
                if (components[i] != null)
                    unityContext.Target.OnMonobehaviourRemoved(components[i]);
        }
    }
}
