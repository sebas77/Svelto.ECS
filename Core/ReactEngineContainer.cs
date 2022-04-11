using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public readonly struct ReactEngineContainer<T> where T:IReactEngine
    {
        public readonly string name;
        public readonly T      engine;

        public ReactEngineContainer(T engine, string name)
        {
            this.name   = name;
            this.engine = engine;
        }
    }
}