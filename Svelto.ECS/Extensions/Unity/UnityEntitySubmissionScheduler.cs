#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Collections;
using Svelto.WeakEvents;
using UnityEngine;

namespace Svelto.ECS.Schedulers.Unity
{
    //The EntitySubmissionScheduler has been introduced to make the entity views submission logic platform independent
    //You can customize the scheduler if you wish
    public class UnityEntitySubmissionScheduler : IEntitySubmissionScheduler
    {
        class Scheduler : MonoBehaviour
        {
            IEnumerator Start()
            {
                while (true)
                {
                    yield return _wait;

                    if (onTick.IsValid)
                        onTick.Invoke();
                    else
                        yield break;

                }
            }

            readonly WaitForEndOfFrame _wait = new WaitForEndOfFrame();
            
            public WeakAction onTick;
        }
        
        public UnityEntitySubmissionScheduler(string name = "ECSScheduler") { _name = name; }
        
        public WeakAction onTick
        {
            set
            {
                if (_scheduler == null)
                {
                    GameObject go = new GameObject(_name);

                    _scheduler = go.AddComponent<Scheduler>();
                }
                _scheduler.onTick = value;
            }
        }

        Scheduler _scheduler;
        string _name;
    }
}
#endif