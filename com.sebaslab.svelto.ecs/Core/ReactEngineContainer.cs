using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public readonly struct ReactEngineContainer
    {
        public readonly string name;
        public readonly         IReactEngine engine;

        public ReactEngineContainer(IReactEngine engine, string name)
        {
            this.name   = name;
            this.engine = engine;
        }
    }
}