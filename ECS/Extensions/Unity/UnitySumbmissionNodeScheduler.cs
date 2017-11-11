#if UNITY_5 || UNITY_5_3_OR_NEWER
using System;
using System.Collections;
using UnityEngine;

namespace Svelto.ECS.NodeSchedulers
{
    //The NodeSubmissionScheduler has been introduced to make
    //the node submission logic platform indipendent.
    //Please don't be tempted to create your own submission to 
    //adapt to your game level code design. For example,
    //you may be tempted to write a submission logic to submit
    //the nodes immediatly just because convenient for your game
    //logic. This is not how it works.
    
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