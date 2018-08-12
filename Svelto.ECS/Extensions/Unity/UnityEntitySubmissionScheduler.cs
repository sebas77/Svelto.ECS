#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Collections;
using Svelto.WeakEvents;
using UnityEngine;

namespace Svelto.ECS.Schedulers.Unity
{
    //The EntitySubmissionScheduler has been introduced to make the entity views submission logic platform independent
    //You can customize the scheduler if you wish
    
    public class UnityEntitySubmissionScheduler : EntitySubmissionScheduler
    {
        public UnityEntitySubmissionScheduler()
        {
            GameObject go = new GameObject("ECSScheduler");

            _scheduler = go.AddComponent<Scheduler>();
        }

        public override void Schedule(WeakAction submitEntityViews)
        {
            _scheduler.OnTick = submitEntityViews;
        }

        class Scheduler : MonoBehaviour
        {
            IEnumerator Start()
            {
                while (true)
                {
                    yield return _wait;

                    if (OnTick.IsValid)
                        OnTick.Invoke();
                    else
                        yield break;
                }
            }

            internal WeakAction OnTick;

            readonly WaitForEndOfFrame _wait = new WaitForEndOfFrame();
        }

        readonly Scheduler _scheduler;
    }
}
#endif