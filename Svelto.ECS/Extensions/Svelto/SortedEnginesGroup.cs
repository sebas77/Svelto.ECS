using Svelto.DataStructures;
using Svelto.Common;

namespace Svelto.ECS.Extensions
{
    public interface IStepEngine : IEngine
    {
        void Step();
        
        string name { get; }
    }
    
    public interface IGroupEngine : IStepEngine
    { }
    
    public interface IStepEngine<T> : IEngine
    {
        void Step(ref T _param);
        
        string name { get; }
    }
    
    public interface IStepGroupEngine<T> : IStepEngine<T>
    {
    }
    /// <summary>
    /// Note sorted jobs run in serial
    /// </summary>
    /// <typeparam name="Interface"></typeparam>
    /// <typeparam name="SequenceOrder"></typeparam>
    public abstract class SortedEnginesGroup<Interface, SequenceOrder> : IGroupEngine
        where SequenceOrder : struct, ISequenceOrder where Interface : class, IStepEngine
    {
        protected SortedEnginesGroup(FasterList<Interface> engines)
        {
            _name = "SortedEnginesGroup - "+this.GetType().Name;
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

        public string name => _name;
        
        readonly string _name;
        readonly Sequence<Interface, SequenceOrder> _instancedSequence;
    } 
    
    public abstract class SortedEnginesGroup<Interface, Parameter, SequenceOrder>: IStepGroupEngine<Parameter>
        where SequenceOrder : struct, ISequenceOrder where Interface : class, IStepEngine<Parameter>
    {
        protected SortedEnginesGroup(FasterList<Interface> engines)
        {
            _name = "SortedEnginesGroup - "+this.GetType().Name;
            _instancedSequence = new Sequence<Interface, SequenceOrder>(engines);
        }

        public void Step(ref Parameter param)
        {
            var sequenceItems = _instancedSequence.items;
            using (var profiler = new PlatformProfiler(_name))
            {
                for (var index = 0; index < sequenceItems.count; index++)
                {
                    var engine = sequenceItems[index];
                    using (profiler.Sample(engine.name)) engine.Step(ref param);
                }
            }
        }

        public string name => _name;
        
        readonly string _name;
        readonly Sequence<Interface, SequenceOrder> _instancedSequence;
    }
}
