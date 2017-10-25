#if UNITY_5 || UNITY_5_3_OR_NEWER
using System;
using System.Collections;
using UnityEngine;

namespace Svelto.ECS.NodeSchedulers
{
    public class UnitySumbmissionNodeScheduler : NodeSubmissionScheduler
    {
        public UnitySumbmissionNodeScheduler()
        {
            GameObject go = new GameObject("ECSScheduler");

            _scheduler = go.AddComponent<Scheduler>();
        }

        public override void Schedule(Action submitNodes)
        {
            _scheduler.OnTick += submitNodes;
        }

        class Scheduler : MonoBehaviour
        {
            IEnumerator Start()
            {
                while (true)
                {
                    yield return _wait;

                    OnTick();
                }
            }

            internal Action OnTick;
            WaitForEndOfFrame _wait = new WaitForEndOfFrame();
        }

        Scheduler _scheduler;
    }
}
#endif