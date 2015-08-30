using UnityEngine;

namespace Svelto.Context
{
	public interface ICompositionRoot
    {
		void OnContextInitialized();
		void OnContextDestroyed();
	}
}


