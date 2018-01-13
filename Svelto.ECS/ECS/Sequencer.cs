using System;
using Steps = System.Collections.Generic.Dictionary<Svelto.ECS.IEngine, System.Collections.Generic.Dictionary<System.Enum, Svelto.ECS.IStep[]>>;

namespace Svelto.ECS
{
    public interface IStep
    { }

    public interface IStep<T>:IStep
    {
        void Step(ref T token, Enum condition);
    }

    public class Sequencer
    {
        public void SetSequence(Steps steps)       
        {
            _steps = steps;
        }

        public void Next<T>(IEngine engine, ref T param)
        {
            Next(engine, ref param, Condition.always);
        }

        public void Next<T>(IEngine engine, ref T param, Enum condition)
        {
            var tos = _steps[engine];
            var steps = tos[condition];

            if (steps != null)
                for (int i = 0; i < steps.Length; i++)
                    ((IStep<T>)steps[i]).Step(ref param, condition);
        }

        Steps     _steps;
    }

    //you can inherit from Condition and add yours
    public enum Condition
    {
        always
    }
}   