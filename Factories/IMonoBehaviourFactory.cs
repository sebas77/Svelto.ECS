using System;
using UnityEngine;

namespace Svelto.Factories
{
	public interface IMonoBehaviourFactory
	{
		M Build<M>(Func<M> constructor) where M:MonoBehaviour;
	}
}

