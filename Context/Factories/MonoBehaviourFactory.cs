#if UNITY_5 || UNITY_5_3_OR_NEWER
#region

using System;
using UnityEngine;

#endregion

namespace Svelto.Context
{
    public class MonoBehaviourFactory : Factories.IMonoBehaviourFactory
    {
        virtual public M Build<M>(Func<M> constructor) where M : MonoBehaviour
        {
            var mb = constructor();

            return mb;
        }
    }
}
#endif