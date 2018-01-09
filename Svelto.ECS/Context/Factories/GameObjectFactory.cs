#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Collections.Generic;
using UnityEngine;

namespace Svelto.Context
{
    public class GameObjectFactory : Factories.IGameObjectFactory
    {
        public GameObjectFactory()
        {
            _prefabs = new Dictionary<string, GameObject[]>();
        }

        public GameObject Build(string prefabName)
        {
            DesignByContract.Check.Require(_prefabs.ContainsKey(prefabName), "Svelto.Factories.IGameObjectFactory -prefab was not found:" + prefabName);

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
        virtual public GameObject Build(GameObject prefab)
        {
            var copy = Object.Instantiate(prefab) as GameObject;

            return copy;
        }

        public void RegisterPrefab(GameObject prefab, string prefabName, GameObject parent = null)
        {
            var objects = new GameObject[2];

            objects[0] = prefab; objects[1] = parent;

            _prefabs.Add(prefabName, objects);
        }

        Dictionary<string, GameObject[]>                        _prefabs;
    }
}
#endif