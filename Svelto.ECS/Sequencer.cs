using System;
using System.Collections;
using System.Collections.Generic;

namespace Svelto.ECS
{
    public class Steps : Dictionary<IEngine, IDictionary>
    {}

    public class To : Dictionary<int, IStep[]>
    {
        public void Add(IStep engine)
        {
            Add(Condition.Always, new [] {engine});
        }
        public void Add(IStep[] engines)
        {
            Add(Condition.Always, engines);
        }
    }
    
    public class To<C> : Dictionary<C, IStep[]> where C:struct,IConvertible
    {}
    
    public interface IStep
    {}

    public interface IStep<T>:IStep
    {
        void Step(ref T token, int condition);
    }
    
    public interface IStep<T, in C>:IStep where C:struct,IConvertible
    {
        void Step(ref T token, C condition);
    }
    
    public interface IEnumStep<T>:IStep
    {
        void Step(ref T token, Enum condition);
    }
    
    public interface ISequencer
    {
        void Next<T>(IEngine engine, ref T param);
        void Next<T>(IEngine engine, ref T param, int condition);
        void Next<T, C>(IEngine engine, ref T param, C condition) where C : struct, IConvertible;
    }

    public class Sequencer : ISequencer
    {
        public void SetSequence(Steps steps)       
        {
            _steps = steps;
        }

        public void Next<T>(IEngine engine, ref T param)
        {
            Next(engine, ref param, Condition.Always);
        }

        public void Next<T>(IEngine engine, ref T param, int condition)
        {
            int branch = condition;
            var steps = (_steps[engine] as Dictionary<int, IStep[]>)[branch];

            if (steps != null)
                for (int i = 0; i < steps.Length; i++)
                    ((IStep<T>)steps[i]).Step(ref param, condition);
        }
        
        public void Next<T>(IEngine engine, ref T param, Enum condition)
        {
            int branch = Convert.ToInt32(condition);
            var steps  = (_steps[engine] as Dictionary<int, IStep[]>)[branch];

            if (steps != null)
                for (int i = 0; i < steps.Length; i++)
                    ((IEnumStep<T>)steps[i]).Step(ref param, condition);
        }

        public void Next<T, C>(IEngine engine, ref T param, C condition) where C:struct,IConvertible
        {
            C branch = condition;
            var steps  = (_steps[engine] as Dictionary<C, IStep[]>)[branch];

            if (steps != null)
                for (int i = 0; i < steps.Length; i++)
                    ((IStep<T, C>)steps[i]).Step(ref param, condition);
        }

        Steps _steps;
    }

    public static class Condition
    {
        public const int Always = 0;
    }
}   