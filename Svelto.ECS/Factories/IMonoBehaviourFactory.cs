#if UNITY_5_3_OR_NEWER || UNITY_5

using System;
using UnityEngine;

namespace Svelto.Factories
{
	public interface IMonoBehaviourFactory
	{
		M Build<M>(Func<M> constructor) where M:MonoBehaviour;
	}
}

#endif