#if UNITY_5 || UNITY_5_3_OR_NEWER
using Object = UnityEngine.Object;
using System.Collections;
using UnityEngine;

namespace Svelto.ECS.Schedulers.Unity
{
    //The EntitySubmissionScheduler has been introduced to make the entity views submission logic platform independent
    //You can customize the scheduler if you wish
    public class UnityEntitiesSubmissionScheduler : IEntitiesSubmissionScheduler
    {
        class Scheduler : MonoBehaviour
        {
            public Scheduler()
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
                    
                    onTick.Invoke();
                }
            }

            readonly WaitForEndOfFrame _wait = new WaitForEndOfFrame();
            readonly IEnumerator       _coroutine;
            
            public EnginesRoot.EntitiesSubmitter onTick;
        }
        
        public UnityEntitiesSubmissionScheduler(string name = "ECSScheduler") { _name = name; }

        public void Dispose()
        {
            if (_scheduler != null && _scheduler.gameObject != null)
            {
                Object.Destroy(_scheduler.gameObject);
            }
        }
        
        public EnginesRoot.EntitiesSubmitter onTick
        {
            set
            {
                if (_scheduler == null)
                {
                    _scheduler = new GameObject(_name).AddComponent<Scheduler>();
                    GameObject.DontDestroyOnLoad(_scheduler.gameObject);
                }

                _scheduler.onTick = value;
            }
        }

        Scheduler       _scheduler;
        readonly string _name;
    }
}
#endif