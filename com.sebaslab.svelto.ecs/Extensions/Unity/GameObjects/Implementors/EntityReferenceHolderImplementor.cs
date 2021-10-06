#if UNITY_5 || UNITY_5_3_OR_NEWER
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Extensions.Unity
{
    [DisallowMultipleComponent]
    public class EntityReferenceHolderImplementor : MonoBehaviour, IImplementor
    {
        public EntityReference reference { get; set; }
    }
}
#endif