#region

using System.Collections.Generic;
using Svelto.Context.Legacy;
using Svelto.DataStructures;
using UnityEngine;

#endregion

namespace Svelto.Context.Legacy
{
    public class GameObjectFactory : Factories.IGameObjectFactory
    {
        public GameObjectFactory(IUnityContextHierarchyChangedListener root)
        {
            _unityContext = new WeakReference<IUnityContextHierarchyChangedListener>(root);

            _prefabs = new Dictionary<string, GameObject[]>();
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
                transform.localScale = scale;
            }

            return go;
        }

        /// <summary>
        /// Register a prefab to be built later using a string ID.
        /// </summary>
        /// <param name="prefab">original prefab</param>
        public GameObject Build(GameObject prefab)
        {
            DesignByContract.Check.Require(_unityContext.IsAlive == true, "Context is used, but not alive");

            UnityEngine.Profiling.Profiler.BeginSample("GameObject Factory Build");

            var copy = Object.Instantiate(prefab) as GameObject;
            var components = copy.GetComponentsInChildren<MonoBehaviour>(true);

            for (var i = 0; i < components.Length; ++i)
            {
                var monoBehaviour = components[i];
  
                if (monoBehaviour != null)
                {
                    var currentGo = monoBehaviour.gameObject;

                    _unityContext.Target.OnMonobehaviourAdded(monoBehaviour);
                    
                    if (currentGo.GetComponent<NotifyComponentsRemoved>() == null)
                        currentGo.AddComponent<NotifyComponentsRemoved>().unityContext = _unityContext;
                }
                else
                {
                    //Utility.Console.Log("delete me");
                }
            }

            UnityEngine.Profiling.Profiler.EndSample();

            return copy;
        }

        public void RegisterPrefab(GameObject prefab, string prefabName, GameObject parent = null)
        {
            var objects = new GameObject[2];

            objects[0] = prefab; objects[1] = parent;

            _prefabs.Add(prefabName, objects);
        }

        Dictionary<string, GameObject[]>                        _prefabs;
        WeakReference<IUnityContextHierarchyChangedListener>    _unityContext;
    }
}