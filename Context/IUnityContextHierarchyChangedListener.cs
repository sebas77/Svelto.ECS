using UnityEngine;

namespace Svelto.Context
{
    public interface IUnityContextHierarchyChangedListener
    {
        void OnMonobehaviourAdded(MonoBehaviour component);
        void OnMonobehaviourRemoved(MonoBehaviour component);

        void OnGameObjectAdded(GameObject entity);
        void OnGameObjectRemoved(GameObject entity);
    }
}
