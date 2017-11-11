#if UNITY_5_3_OR_NEWER || UNITY_5
using UnityEngine;

namespace Svelto.Factories
{
    public interface IGameObjectFactory
    {
        void RegisterPrefab(GameObject prefab, string type, GameObject parent = null);

        GameObject Build(string type);
        GameObject Build(GameObject prefab);
    }
}
#endif