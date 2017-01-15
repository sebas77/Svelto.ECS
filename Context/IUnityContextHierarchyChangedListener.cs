using UnityEngine;

namespace Svelto.Context.Legacy
{
    public interface IUnityContextHierarchyChangedListener
    {
        void OnMonobehaviourAdded(MonoBehaviour component);

        void OnMonobehaviourRemoved(MonoBehaviour component);
    }
}
