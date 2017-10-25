#if UNITY_EDITOR
using UnityEditor;

//This profiler is based on the Entitas Visual Debugging tool 
//https://github.com/sschmid/Entitas-CSharp

namespace Svelto.ECS.Profiler
{
    class EngineProfilerMenuItem
    {
        [MenuItem("Engines/Enable Profiler")]
        public static void EnableProfiler()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "ENGINE_PROFILER_ENABLED");
        }

        [MenuItem("Engines/Disable Profiler")]
        public static void DisableProfiler()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");
        }
    }
}
#endif