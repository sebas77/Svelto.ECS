using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Svelto.Ticker.Legacy;

//This profiler is based on the Entitas Visual Debugging tool 
//https://github.com/sschmid/Entitas-CSharp

namespace Svelto.ECS.Profiler
{
    public sealed class EngineProfiler
    {
        static readonly Stopwatch _stopwatch = new Stopwatch();

        public static void MonitorAddDuration(Action<INodeEngine<INode>, INode> addingFunc, INodeEngine<INode> engine, INode node)
        {
            EngineInfo info;
            if (engineInfos.TryGetValue(engine.GetType(), out info))
            {
                _stopwatch.Reset();
                _stopwatch.Start();
                addingFunc(engine, node);
                _stopwatch.Stop();

                info.AddAddDuration(_stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        public static void MonitorRemoveDuration(Action<INodeEngine<INode>, INode> removeFunc, INodeEngine<INode> engine, INode node)
        {
            EngineInfo info;
            if (engineInfos.TryGetValue(engine.GetType(), out info))
            {
                _stopwatch.Reset();
                _stopwatch.Start();
                removeFunc(engine, node);
                engine.Remove(node);
                _stopwatch.Stop();
            
                info.AddRemoveDuration(_stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        public static void MonitorUpdateDuration(ITickable tickable)
        {
            if (tickable is INodeEngine<INode>)
            {
                EngineInfo info;
                if (engineInfos.TryGetValue((tickable as INodeEngine<INode>).GetType(), out info))
                {
                    _stopwatch.Reset();
                    _stopwatch.Start();
                    tickable.Tick(Time.deltaTime);
                    _stopwatch.Stop();

                    info.AddUpdateDuration(_stopwatch.Elapsed.TotalMilliseconds);
                }
            }
            else
            {
                tickable.Tick(Time.deltaTime);
            }
        }

        public static void MonitorUpdateDuration(IPhysicallyTickable tickable)
        {
            if (tickable is INodeEngine<INode>)
            {
                EngineInfo info;
                if (engineInfos.TryGetValue((tickable as INodeEngine<INode>).GetType(), out info))
                {
                    _stopwatch.Reset();
                    _stopwatch.Start();
                    tickable.PhysicsTick(Time.fixedDeltaTime);
                    _stopwatch.Stop();

                    info.AddFixedUpdateDuration(_stopwatch.Elapsed.TotalMilliseconds);
                }
            }
            else
            {
                tickable.PhysicsTick(Time.fixedDeltaTime);
            }
        }

        public static void MonitorUpdateDuration(ILateTickable tickable)
        {
            if (tickable is INodeEngine<INode>)
            {
                EngineInfo info;
                if (engineInfos.TryGetValue((tickable as INodeEngine<INode>).GetType(), out info))
                {
                    _stopwatch.Reset();
                    _stopwatch.Start();
                    tickable.LateTick(Time.deltaTime);
                    _stopwatch.Stop();

                    info.AddLateUpdateDuration(_stopwatch.Elapsed.TotalMilliseconds);
                }
            }
            else
            {
                tickable.LateTick(Time.deltaTime);
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
