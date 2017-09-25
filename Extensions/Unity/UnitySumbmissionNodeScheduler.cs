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
                    yield return new WaitForEndOfFrame();

                    OnTick();
                }
            }

            internal Action OnTick;
        }

        Scheduler _scheduler;
    }
}
