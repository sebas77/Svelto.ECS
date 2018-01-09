using System;
using System.Collections.Generic;
using System.Diagnostics;
using Svelto.ECS.Internal;

//This profiler is based on the Entitas Visual Debugging tool 
//https://github.com/sschmid/Entitas-CSharp

namespace Svelto.ECS.Profiler
{
    public sealed class EngineProfiler
    {
        static readonly Stopwatch _stopwatch = new Stopwatch();

        public static void MonitorAddDuration(IHandleEntityViewEngine engine, IEntityView entityView)
        {
            EngineInfo info;
            if (engineInfos.TryGetValue(engine.GetType(), out info))
            {
                _stopwatch.Start();
                engine.Add(entityView);
                _stopwatch.Reset();

                info.AddAddDuration(_stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        public static void MonitorRemoveDuration(IHandleEntityViewEngine engine, IEntityView entityView)
        {
            EngineInfo info;
            if (engineInfos.TryGetValue(engine.GetType(), out info))
            {
                _stopwatch.Start();
                engine.Remove(entityView);
                _stopwatch.Reset();

                info.AddRemoveDuration(_stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        public static void AddEngine(IEngine engine)
        {
            if (engineInfos.ContainsKey(engine.GetType()) == false)
            {
                engineInfos.Add(engine.GetType(), new EngineInfo(engine));
            }
        }

        public static void ResetDurations()
        {
            foreach (var engine in engineInfos)
            {
                engine.Value.ResetDurations();
            }
        }

        public static readonly Dictionary<Type, EngineInfo> engineInfos = new Dictionary<Type, EngineInfo>();
    }
}
