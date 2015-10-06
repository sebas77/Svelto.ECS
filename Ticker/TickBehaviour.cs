using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Svelto.Ticker
{
	public class TickBehaviour:MonoBehaviour
	{
		internal void Add(ITickable tickable)
		{
			_ticked.Add(tickable);
		}
		
		internal void Remove(ITickable tickable)
		{
			_ticked.Remove(tickable);
		}

		internal void AddPhysic(IPhysicallyTickable tickable)
		{
			_physicallyTicked.Add(tickable);
		}

		internal void RemovePhysic(IPhysicallyTickable tickable)
		{
			_physicallyTicked.Remove(tickable);
		}

		internal void AddLate(ILateTickable tickable)
		{
			_lateTicked.Add(tickable);
		}

		internal void RemoveLate(ILateTickable tickable)
		{
			_lateTicked.Remove(tickable);
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

        void Update()
		{
            for (int i = _ticked.Count - 1; i >= 0; --i)
				_ticked[i].Tick(Time.deltaTime);
		}
		void LateUpdate()
		{
			for (int i = _lateTicked.Count - 1; i >= 0; --i)
				_lateTicked[i].LateTick(Time.deltaTime);
		}
		void FixedUpdate()
		{
			for (int i = _physicallyTicked.Count - 1; i >= 0; --i)
				_physicallyTicked[i].PhysicsTick(Time.deltaTime);
		}
        IEnumerator IntervaledUpdate(IIntervaledTickable tickable, float seconds)
        {
            while (true) { DateTime next = DateTime.UtcNow.AddSeconds(seconds); while (DateTime.UtcNow < next) yield return null; tickable.IntervaledTick(); }
        }

        private List<ITickable> _ticked = new List<ITickable>();
		private List<ILateTickable> _lateTicked = new List<ILateTickable>();
		private List<IPhysicallyTickable> _physicallyTicked = new List<IPhysicallyTickable>();
        private Dictionary<IIntervaledTickable, IEnumerator> _intervalledTicked = new Dictionary<IIntervaledTickable, IEnumerator>();
	}
}
