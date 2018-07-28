using System;
using System.Collections;
using System.Collections.Generic;

namespace Svelto.ECS
{
    public class Steps : Dictionary<IEngine, IDictionary>
    {}

    public class To<C> : Dictionary<C, IStep[]> where C : struct, IConvertible
    {
        public void Add(C condition, IStep engine)
        {
            Add(condition, new [] {engine});
        }
        public void Add(C condition, params IStep[] engines)
        {
            Add(condition, engines);
        }
        public void Add(params IStep[] engines)
        {
            Add(default(C), engines);
        }
    }
    
    public interface IStep
    {}

    public interface IStep<in C>:IStep where C:struct,IConvertible
    {
        void Step(C condition, EGID id);
    }

    public abstract class Sequencer
    {
        public void SetSequence(Steps steps)       
        {
            _steps = steps;
        }

        public void Next(IEngine engine, EGID id)
        {
            Next(engine, Condition.Always, id);
        }
        
        public void Next(IEngine engine)
        {
            Next(engine, Condition.Always);
        }

        public void Next<C>(IEngine engine, C condition, EGID id = new EGID()) where C:struct,IConvertible
        {
            C branch = condition;
            var steps  = (_steps[engine] as Dictionary<C, IStep[]>)[branch];

            if (steps == null) return;
            
            for (var i = 0; i < steps.Length; i++)
                ((IStep<C>)steps[i]).Step(condition, id);
        }

        Steps _steps;
    }

    public static class Condition
    {
        public const int Always = 0;
    }
}       