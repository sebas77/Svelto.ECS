using UnityEngine;

namespace Svelto.Ticker
{
	internal class UnityTicker: ITicker
	{
		public UnityTicker()
		{
            _ticker = GameObject.FindObjectOfType<TickBehaviour>();

            if (_ticker == null)
			{
                GameObject go = new GameObject("SveltoTicker");
				
				_ticker = go.AddComponent<TickBehaviour>();
			}
		}
		
		public void Add(ITickableBase tickable)
		{
            if (tickable is ITickable)
			    _ticker.Add(tickable as ITickable);

            if (tickable is IPhysicallyTickable)
                _ticker.AddPhysic(tickable as IPhysicallyTickable);

            if (tickable is ILateTickable)
                _ticker.AddLate(tickable as ILateTickable);

            if (tickable is IIntervaledTickable)
                _ticker.AddIntervaled(tickable as IIntervaledTickable);
        }
		
		public void Remove(ITickableBase tickable)
		{
            if (tickable is ITickable)
                _ticker.Remove(tickable as ITickable);

            if (tickable is IPhysicallyTickable)
                _ticker.RemovePhysic(tickable as IPhysicallyTickable);

            if (tickable is ILateTickable)
                _ticker.RemoveLate(tickable as ILateTickable);

            if (tickable is IIntervaledTickable)
                _ticker.RemoveIntervaled(tickable as IIntervaledTickable);
        }

		private TickBehaviour 	_ticker;
	}
}
	



 
