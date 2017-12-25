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

        public static void MonitorAddDuration(INodeEngine engine, INode node)
        {
            EngineInfo info;
            if (engineInfos.TryGetValue(engine.GetType(), out info))
            {
                _stopwatch.Reset();
                _stopwatch.Start();
                engine.Add(node);
                _stopwatch.Stop();

                info.AddAddDuration(_stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        public static void MonitorRemoveDuration(INodeEngine engine, INode node)
        {
            EngineInfo info;
            if (engineInfos.TryGetValue(engine.GetType(), out info))
            {
                _stopwatch.Reset();
                _stopwatch.Start();
                engine.Remove(node);
                _stopwatch.Stop();
            
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
