using System;
using System.Collections;
using System.Collections.Generic;

namespace Svelto.ECS
{
    public class Steps
    {
       internal readonly Dictionary<IEngine, To> _steps;

        public Steps(params Step[] values)
        {
            _steps = new Dictionary<IEngine, To>();

            for (int i = 0; i < values.Length; i++)
                _steps.Add(values[i].from, values[i].to);
        }
    }

    public class To 
    {
        public To(IStep engine)
        {
            this.engine = engine;
        }
        
        public To(params IStep[] engines)
        {
            this.engines = engines;
        }

        public IStep engine { get; set; }
        public IStep[] engines { get; set; }
    }

    public class To<C>:To, IEnumerable where C : struct, IConvertible
    {
        internal readonly Dictionary<C, IStep<C>[]> _tos = new Dictionary<C, IStep<C>[]>();

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Add(C condition, params IStep<C>[] engine)
        {
            _tos[condition] = engine;
        }
    }

    public interface IStep
    {
        void Step(EGID id);
    }
    
    public interface IStep<in C> where C:struct,IConvertible
    {
        void Step(C condition, EGID id);
    }

    public struct Step
    {
        public IEngine from { get; set; }
        public To to { get; set; }
    }
    
    /// <summary>
    /// The sequencer has just one goal: define a complex sequence of engines to call. The sequence is not
    /// just "sequential", but can create branches and loops.
    /// With the time, I figure out that the majority of the times this class is useful in the rare cases where
    /// order execution of the engine is necessary/
    /// Using branching is even rarer, but still useful sometimes.
    /// I used loops only once.
    /// There is the chance that this class will become obsolete over the time, as by design engines should
    /// never rely on the order of execution
    /// Using this class to let engines from different EnginesRoot communicate will also become obsolete, as
    /// better solution will be implemented once I have the time
    /// Trying to work out how to initialize this structure can be painful. This is by design as this class must
    /// be initialized using the following pattern:
    ///    instancedSequence.SetSequence(
    ///                         new Steps //list of steps
    ///                             (
    ///                              new Step // first step 
    ///                              {
    ///                                  from = menuOptionEnablerEngine,                //starting engine
    ///                                  to = new To<ItemActionsPanelEnableCondition>   //targets
    ///                                      {
    ///                                           {
    ///                                               ItemActionsPanelEnableCondition.Enable, //condition 1
    ///                                               menuPanelEnablerEngine                  //targets for condition 1
    ///                                           },
    ///                                           {
    ///                                               ItemActionsPanelEnableCondition.Disable,//condition 2
    ///                                               menuPanelEnablerEngine                  //targets for condition 2
    ///                                           }
    ///                                      }
    ///                              })
    ///                    ); 
    /// </summary>
    public class Sequencer<S> where S: Sequencer<S>, new()
    {
        protected void SetSequence(Steps steps)       
        {
            _steps = steps;
        }

        public void Next<C>(IEngine engine, C condition, EGID id) where C:struct, IConvertible
        {
            C branch = condition;
            var to = (_steps._steps[engine] as To<C>);
            
            var steps  = to._tos[branch];

            for (var i = 0; i < steps.Length; i++)
                steps[i].Step(condition, id);
        }
        
        public void Next(IEngine engine, EGID id)
        {
            var to  = _steps._steps[engine];
            var steps = to.engines;
            
            if (steps != null && steps.Length > 1)
                for (var i = 0; i < steps.Length; i++)
                    steps[i].Step(id);
            else
                to.engine.Step(id);
        }

        Steps _steps;
    }
}       