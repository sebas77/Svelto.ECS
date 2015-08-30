#region

using System;
using UnityEngine;

#endregion

namespace Svelto.Context
{
    public class MonoBehaviourFactory: Factories.IMonoBehaviourFactory
	{
        IUnityContextHierarchyChangedListener _unityContext;

		public MonoBehaviourFactory(IUnityContextHierarchyChangedListener unityContext)
		{
			_unityContext = unityContext;
		}
		
		public M Build<M>(Func<M> constructor) where M:MonoBehaviour
		{
			var mb = constructor();
			
			_unityContext.OnMonobehaviourAdded(mb);

            GameObject go = mb.gameObject;

            if (go.GetComponent<NotifyComponentsRemoved>() == null)
                go.AddComponent<NotifyComponentsRemoved>().unityContext = _unityContext;

            return mb;
		}
	}
}

