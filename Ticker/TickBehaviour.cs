using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Svelto.Ticker
{
    public class TickBehaviour : MonoBehaviour
    {
        IEnumerator Start () 
        {
            var waitForEndFrame = new WaitForEndOfFrame();

            while (true)
            {
                yield return waitForEndFrame;

                for (int i = _endOfFrameTicked.Count - 1; i >= 0; --i)
                {
                    try
                    {
                        _endOfFrameTicked[i].EndOfFrameTick(Time.deltaTime);
                    }
                    catch (Exception e)
                    {
                        Utility.Console.LogException(e);
                    }
                }
            }
        }
 
        internal void Add(ITickable tickable)
        {
            _ticked.Add(tickable);
        }

        internal void AddLate(ILateTickable tickable)
        {
            _lateTicked.Add(tickable);
        }

        internal void AddPhysic(IPhysicallyTickable tickable)
        {
            _physicallyTicked.Add(tickable);
        }

        internal void AddEndOfFrame(IEndOfFrameTickable tickable)
        {
            _endOfFrameTicked.Add(tickable);
        }

        internal void Remove(ITickable tickable)
        {
            _ticked.Remove(tickable);
        }

        internal void RemoveLate(ILateTickable tickable)
        {
            _lateTicked.Remove(tickable);
        }

        internal void RemovePhysic(IPhysicallyTickable tickable)
        {
            _physicallyTicked.Remove(tickable);
        }

        internal void RemoveEndOfFrame(IEndOfFrameTickable tickable)
        {
            _endOfFrameTicked.Remove(tickable);
        }

        void FixedUpdate()
        {
            for (int i = _physicallyTicked.Count - 1; i >= 0; --i)
            {
                try
                {
                    _physicallyTicked[i].PhysicsTick(Time.fixedDeltaTime);
                }
                catch (Exception e)
                {
                    Utility.Console.LogException(e);
                }
            }
        }

        void LateUpdate()
        {
            for (int i = _lateTicked.Count - 1; i >= 0; --i)
            {
                try
                {
                    _lateTicked[i].LateTick(Time.deltaTime);
                }
                catch (Exception e)
                {
                    Utility.Console.LogException(e);
                }
            }
        }

        void Update()
        {
            for (int i = _ticked.Count - 1; i >= 0; --i)
            {
                try
                {
                    _ticked[i].Tick(Time.deltaTime);
                }
                catch (Exception e)
                {
                    Utility.Console.LogException(e);
                }
            }
        }

        internal void AddIntervaled(IIntervaledTickable tickable)
        {
            var methodInfo = ((Action)tickable.IntervaledTick).Method;
            object[] attrs = methodInfo.GetCustomAttributes(typeof(IntervaledTickAttribute), true);

            IEnumerator intervaledTick = IntervaledUpdate(tickable, (attrs[0] as IntervaledTickAttribute).interval);

            _intervalledTicked[tickable] = intervaledTick;

            StartCoroutine(intervaledTick);
        }

        internal void RemoveIntervaled(IIntervaledTickable tickable)
        {
            IEnumerator enumerator;

            if (_intervalledTicked.TryGetValue(tickable, out enumerator))
            {
                StopCoroutine(enumerator);

                _intervalledTicked.Remove(tickable);
            }
        }

        IEnumerator IntervaledUpdate(IIntervaledTickable tickable, float seconds)
        {
            while (true) { DateTime next = DateTime.UtcNow.AddSeconds(seconds); while (DateTime.UtcNow < next) yield return null; tickable.IntervaledTick(); }
        }

        List<ILateTickable>         _lateTicked = new List<ILateTickable>();
        List<IPhysicallyTickable>   _physicallyTicked = new List<IPhysicallyTickable>();
        List<ITickable>             _ticked = new List<ITickable>();
        List<IEndOfFrameTickable>   _endOfFrameTicked = new List<IEndOfFrameTickable>();

        Dictionary<IIntervaledTickable, IEnumerator> _intervalledTicked = new Dictionary<IIntervaledTickable, IEnumerator>();
    }
}
