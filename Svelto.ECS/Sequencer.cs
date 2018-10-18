using System;
using System.Collections;
using System.Collections.Generic;

namespace Svelto.ECS
{
    public class Steps : Dictionary<IEngine, IDictionary>
    {
        public new void Add(IEngine engine, IDictionary dictionary)
        {
            if (ContainsKey(engine))
            {
                Svelto.Utilities.Console.LogError("can't hold multiple steps with the same engine as origin in a Sequencer");
            }
            base.Add(engine, dictionary);
        }
    }

    public class To<C> : Dictionary<C, IStep<C>[]> where C : struct, IConvertible
    {
        public new void Add(C condition, params IStep<C>[] engines)
        {
            base.Add(condition, engines);
        }
    }
    
    public interface IStep<in C> where C:struct,IConvertible
    {
        void Step(C condition, EGID id);
    }

    public abstract class Sequencer
    {
        public void SetSequence(Steps steps)       
        {
            _steps = steps;
        }

        public void Next<C>(IEngine engine, C condition, EGID id) where C:struct,IConvertible
        {
            C branch = condition;
            var steps  = (_steps[engine] as Dictionary<C, IStep<C>[]>)[branch];

            if (steps == null)
                Svelto.Utilities.Console.LogError("selected steps not found in sequencer ".FastConcat(this.ToString()));
            
            for (var i = 0; i < steps.Length; i++)
                steps[i].Step(condition, id);
        }

        Steps _steps;
    }
}       