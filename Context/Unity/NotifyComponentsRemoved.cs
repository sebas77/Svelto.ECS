using UnityEngine;

namespace Svelto.Context
{
    public class NotifyComponentsRemoved : MonoBehaviour
    {
        public IUnityContextHierarchyChangedListener unityContext { private get; set; }

        void OnDestroy()
        {
            MonoBehaviour[] components = gameObject.GetComponentsInChildren<MonoBehaviour>(true);

            for (int i = 0; i < components.Length; ++i)
                if (components[i] != null)
                    unityContext.OnMonobehaviourRemoved(components[i]);
        }
    }

    public class NotifyEntityRemoved : MonoBehaviour
    {
        public IUnityContextHierarchyChangedListener unityContext { private get; set; }

        void OnDestroy()
        {
            unityContext.OnGameObjectRemoved(gameObject);
        }
    }
}
