using System;
using System.Collections.Generic;

namespace Svelto.ECS
{
    public class Steps : Dictionary<IEngine, Dictionary<int, IStep[]>>
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
    
    public interface IStep
    {}

    public interface IStep<T>:IStep
    {
        void Step(ref T token, int condition);
    }
    
    public interface ISequencer
    {
        void Next<T>(IEngine engine, ref T param);

        void Next<T>(IEngine engine, ref T param, int condition);
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
            var steps = _steps[engine][branch];

            if (steps != null)
                for (int i = 0; i < steps.Length; i++)
                    ((IStep<T>)steps[i]).Step(ref param, condition);
        }

        Steps _steps;
    }

    public static class Condition
    {
        public const int Always = 0;
    }
}   