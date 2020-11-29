#if UNITY_5 || UNITY_5_3_OR_NEWER
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Extensions.Unity
{
    public interface IEGIDHolder
    {
        EGID ID { set; }
    }

    public struct EGIDTrackerViewComponent : IEntityViewComponent
    {
#pragma warning disable 649
        public IEGIDHolder holder;
#pragma warning restore 649
        
        EGID _ID;

        public EGID ID
        {
            get => _ID;
            set => _ID = holder.ID = value;
        }
    }
    
    public class EGIDHolderImplementor : MonoBehaviour, IEGIDHolder, IImplementor
    {
        public EGID ID { get; set; }
    }
}
#endif