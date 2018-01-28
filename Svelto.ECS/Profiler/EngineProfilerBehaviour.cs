#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }

        void OnLevelFinishedLoading(Scene arg0, LoadSceneMode arg1)
        {
            ResetDurations();
        }
    }
}
#endif