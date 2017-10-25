#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

//This profiler is based on the Entitas Visual Debugging tool 
//https://github.com/sschmid/Entitas-CSharp

namespace Svelto.ECS.Profiler
{
    public class EngineProfilerBehaviour : MonoBehaviour
    {
        public Dictionary<Type, EngineInfo>.ValueCollection engines { get { return EngineProfiler.engineInfos.Values; } }

        public void ResetDurations()
        {
            EngineProfiler.ResetDurations();
        }
    }
}
#endif