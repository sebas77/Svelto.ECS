using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    /// <summary>
    /// UnsortedEnginesGroup is a practical way to group engines that can be ticked together. As the name suggest
    /// there is no way to defines an order, although the engines will run in the same order they are added.
    /// It is abstract and it requires a user defined interface to push the user to use recognisable names meaningful
    /// to the context where they are used.
    /// </summary>
    /// <typeparam name="Interface">user defined interface that implements IStepEngine</typeparam>
    public abstract class UnsortedEnginesGroup<Interface> : IStepGroupEngine
        where Interface : IStepEngine
    {
        protected UnsortedEnginesGroup()
        {
            _name              = "UnsortedEnginesGroup - "+this.GetType().Name;
            _instancedSequence = new FasterList<Interface>();
        }
        
        protected UnsortedEnginesGroup(FasterList<Interface> engines)
        {
            _name              = "UnsortedEnginesGroup - "+this.GetType().Name;
            _instancedSequence = engines;
        }

        public void Step()
        {
            var sequenceItems = _instancedSequence;
            using (var profiler = new PlatformProfiler(_name))
            {
                for (var index = 0; index < sequenceItems.count; index++)
                {
                    var engine = sequenceItems[index];
                    using (profiler.Sample(engine.name)) engine.Step();
                }
            }
        }
        
        public void Add(Interface engine)
        {
            _instancedSequence.Add(engine);
        }

        public string name => _name;
        
        readonly string                _name;
        readonly FasterList<Interface> _instancedSequence;
    }
    
    /// <summary>
    /// Similar to UnsortedEnginesGroup except for the fact that an optional parameter can be passed to the engines
    /// </summary>
    /// <typeparam name="Interface"></typeparam>
    /// <typeparam name="Parameter">Specialised Parameter that can be passed to all the engines in the group</typeparam>
    public abstract class UnsortedEnginesGroup<Interface, Parameter> : IStepGroupEngine<Parameter>
        where Interface : IStepEngine<Parameter>
    {
        protected UnsortedEnginesGroup()
        {
            _name              = "UnsortedEnginesGroup - "+this.GetType().Name;
            _instancedSequence = new FasterList<Interface>();
        }
        
        protected UnsortedEnginesGroup(FasterList<Interface> engines)
        {
            _name              = "UnsortedEnginesGroup - "+this.GetType().Name;
            _instancedSequence = engines;
        }

        public void Step(in Parameter param)
        {
            var sequenceItems = _instancedSequence;
            using (var profiler = new PlatformProfiler(_name))
            {
                for (var index = 0; index < sequenceItems.count; index++)
                {
                    var engine = sequenceItems[index];
                    using (profiler.Sample(engine.name)) engine.Step(param);
                }
            }
        }
        
        public void Add(Interface engine)
        {
            _instancedSequence.Add(engine);
        }

        public string name => _name;
        
        readonly string                _name;
        readonly FasterList<Interface> _instancedSequence;
    }
}