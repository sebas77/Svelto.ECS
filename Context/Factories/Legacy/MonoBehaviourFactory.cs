#region

using System;
using Svelto.DataStructures;
using UnityEngine;

#endregion

namespace Svelto.Context.Legacy
{
    public class MonoBehaviourFactory : Factories.IMonoBehaviourFactory
    {
        public MonoBehaviourFactory(IUnityContextHierarchyChangedListener unityContext)
        {
            _unityContext = new WeakReference<IUnityContextHierarchyChangedListener>(unityContext);
        }

        public M Build<M>(Func<M> constructor) where M : MonoBehaviour
        {
            DesignByContract.Check.Require(_unityContext.IsAlive == true, "Context is used, but not alive");

            var mb = constructor();

            _unityContext.Target.OnMonobehaviourAdded(mb);

            GameObject go = mb.gameObject;

            if (go.GetComponent<NotifyComponentsRemoved>() == null)
                go.AddComponent<NotifyComponentsRemoved>().unityContext = _unityContext;

            return mb;
        }

        WeakReference<IUnityContextHierarchyChangedListener> _unityContext;
    }
}
