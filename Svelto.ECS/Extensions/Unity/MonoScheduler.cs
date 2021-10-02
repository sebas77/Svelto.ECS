#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Collections;
using UnityEngine;

namespace Svelto.ECS.Schedulers.Unity
{
    class MonoScheduler : MonoBehaviour
    {
        public MonoScheduler()
        {
            _coroutine = Coroutine();
        }

        void Update()
        {
            _coroutine.MoveNext();
        }
            
        IEnumerator Coroutine()
        {
            while (true)
            {
                yield return _wait;
                    
                onTick();
            }
        }

        readonly WaitForEndOfFrame _wait = new WaitForEndOfFrame();
        readonly IEnumerator       _coroutine;
            
        internal System.Action onTick;
    }
}
#endif