#region

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Svelto.Context
{
    public class GameObjectFactory: Factories.IGameObjectFactory
	{
		public GameObjectFactory(IUnityContextHierarchyChangedListener root)
		{
			_unityContext = root;

			_prefabs = new Dictionary<string, GameObject[]>();
		} 
		
        /// <summary>
        /// Register a prefab to be built later using a string ID. 
        /// </summary>
        /// <param name="prefab">original prefab</param>
        /// <param name="prefabName">prefab name</param>
        /// <param name="parent">optional gameobject to specify as parent later</param>

		public void RegisterPrefab(GameObject prefab, string prefabName, GameObject parent = null)
		{
		 	var objects = new GameObject[2];
		 	
		 	objects[0] = prefab; objects[1] = parent;
			
			_prefabs.Add(prefabName, objects);
		}
		
		public GameObject Build(string prefabName)
		{
			DesignByContract.Check.Require(_prefabs.ContainsKey(prefabName), "Svelto.Factories.IGameObjectFactory - Invalid Prefab Type");
			
			var go = Build(_prefabs[prefabName][0]);
			
            GameObject parent = _prefabs[prefabName][1];

            if (parent != null)
            {
                Transform transform = go.transform;

                var scale = transform.localScale;
                var rotation = transform.localRotation;
                var position = transform.localPosition;

                parent.SetActive(true);

                transform.parent = parent.transform;

                transform.localPosition = position;
                transform.localRotation = rotation;
                transform.localScale    = scale;
            }
			
			return go;
		}

		public GameObject Build(GameObject go)
		{
			var copy = Object.Instantiate(go) as GameObject;
			var components = copy.GetComponentsInChildren<MonoBehaviour>(true);

            for (var i = 0; i < components.Length; ++i)
                if (components[i] != null)
                    _unityContext.OnMonobehaviourAdded(components[i]);

            _unityContext.OnGameObjectAdded(copy);

            copy.AddComponent<NotifyComponentsRemoved>().unityContext = _unityContext;
            copy.AddComponent<NotifyEntityRemoved>().unityContext = _unityContext;

            return copy;
		}

        IUnityContextHierarchyChangedListener   _unityContext;
        Dictionary<string, GameObject[]>        _prefabs;
    }
}

