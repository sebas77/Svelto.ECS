using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.Common;

namespace Svelto.ECS
{
    /// <summary>
    /// SortedEnginesGroup is a practical way to group engines that need to be ticked in order. The class requires a
    /// SequenceOrder struct that defines the order of execution. The pattern to use is the following:
    /// First define as many enums as you want with the IDs of the engines to use. E.G.:
    /// public enum WiresCompositionEngineNames
    ///{
    ///    WiresTimeRunningGroup,
    ///    WiresPreInitTimeRunningGroup,
    ///    WiresInitTimeRunningGroup
    ///}
    ///
    /// then link these ID to the actual engines, using the attribute Sequenced:
    ///
    ///  [Sequenced(nameof(WiresCompositionEngineNames.WiresTimeRunningGroup))]
    ///  class WiresTimeRunningGroup : UnsortedDeterministicEnginesGroup<IDeterministicTimeRunning> {}
    ///
    /// Note that the engine can be another engines group itself.
    /// 
    /// then define the ISequenceOrder struct to define the order of execution. E.G.:
    ///     public struct DeterministicTimeRunningEnginesOrder: ISequenceOrder
    ///     {
    ///         private static readonly string[] order =
    ///         {
    ///             nameof(TriggerEngineNames.PreWiresTimeRunningTriggerGroup),
    ///             nameof(TimerEnginesNames.PreWiresTimeRunningTimerGroup),
    ///             nameof(WiresCompositionEngineNames.WiresTimeRunningGroup),
    ///             nameof(SyncGroupEnginesGroups.UnsortedDeterministicTimeRunningGroup)
    ///          };
    ///         public string[] enginesOrder => order;
    ///      }
    ///
    /// Now you can use the Type you just created (i.e.: DeterministicTimeRunningEnginesOrder) as generic parameter
    /// of the SortedEnginesGroup. like this:
    ///
    /// public class SortedDoofusesEnginesExecutionGroup : SortedEnginesGroup<IUpdateEngine, DeterministicTimeRunningEnginesOrder>
    /// {
    ///     public SortedDoofusesEnginesExecutionGroup(FasterList<IUpdateEngine> engines) : base(engines)
    ///     {
    ///     }
    /// }
    ///
    /// This will then Tick the engines passed by constructor with the order defined in the DeterministicTimeRunningEnginesOrder
    /// calling _enginesGroup.Step();
    /// 
    /// While the system may look convoluted, is an effective way to keep the engines assemblies decoupled from
    /// each other 
    /// The class is abstract and it requires a user defined interface to push the user to use recognisable names meaningful
    /// to the context where they are used.
    /// </summary>
    /// <typeparam name="Interface">user defined interface that implements IStepEngine</typeparam>
    public abstract class SortedEnginesGroup<Interface, SequenceOrder> : IStepGroupEngine
        where SequenceOrder : struct, ISequenceOrder where Interface : class, IStepEngine
    {
        protected SortedEnginesGroup(FasterList<Interface> engines)
        {
            _name = "SortedEnginesGroup - "+GetType().Name;
            _instancedSequence = new Sequence<Interface, SequenceOrder>(engines);
        }

        public void Step()
        {
            var sequenceItems = _instancedSequence.items;
            using (var profiler = new PlatformProfiler(_name))
            {
                for (var index = 0; index < sequenceItems.count; index++)
                {
                    var engine = sequenceItems[index];
                    using (profiler.Sample(engine.name)) engine.Step();
                }
            }
        }

        public string name   => _name;
        public IEnumerable<IEngine> engines
        {
            get
            {
                for (int i = 0; i < _instancedSequence.items.count; i++)
                    yield return _instancedSequence.items[i];
            }
        }

        readonly string _name;
        readonly Sequence<Interface, SequenceOrder> _instancedSequence;
    } 
    
    /// <summary>
    /// Similar to SortedEnginesGroup except for the fact that an optional parameter can be passed to the engines
    /// </summary>
    /// <typeparam name="Interface"></typeparam>
    /// <typeparam name="Parameter">Specialised Parameter that can be passed to all the engines in the group</typeparam>
    public abstract class SortedEnginesGroup<Interface, Parameter, SequenceOrder>: IStepGroupEngine<Parameter>
        where SequenceOrder : struct, ISequenceOrder where Interface : class, IStepEngine<Parameter>
    {
        protected SortedEnginesGroup(FasterList<Interface> engines)
        {
            _name = "SortedEnginesGroup - "+GetType().Name;
            _instancedSequence = new Sequence<Interface, SequenceOrder>(engines);
        }

        public void Step(in Parameter time)
        {
            var sequenceItems = _instancedSequence.items;
            using (var profiler = new PlatformProfiler(_name))
            {
                for (var index = 0; index < sequenceItems.count; index++)
                {
                    var engine = sequenceItems[index];
                    using (profiler.Sample(engine.name)) engine.Step(time);
                }
            }
        }

        public string name => _name;
        public IEnumerable<IEngine> engines
        {
            get
            {
                for (int i = 0; i < _instancedSequence.items.count; i++)
                    yield return _instancedSequence.items[i];
            }
        }
        
        readonly string _name;
        readonly Sequence<Interface, SequenceOrder> _instancedSequence;
    }
}
