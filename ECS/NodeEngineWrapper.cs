using System;

namespace Svelto.ECS.Internal
{
    class NodeEngineWrapper<T> : SingleNodeEngine<T> where T : class, INode
    {
        INodeEngine<T> engine;

        public NodeEngineWrapper(INodeEngine<T> engine)
        {
            this.engine = engine;
        }

        protected override void Add(T node)
        {
            engine.Add((T)node);
        }

        protected override void Remove(T node)
        {
            engine.Remove((T)node);
        }
    }

    class NodeEngineWrapper<T, U>: SingleNodeEngine<T> where T : class, INode where U : class, INode
    {
        INodeEngine<T, U> engine;

        public NodeEngineWrapper(INodeEngine<T, U> engine)
        {
            this.engine = engine;
        }

        protected override void Add(T node)
        {
            engine.Add((T)node);
        }

        protected override void Remove(T node)
        {
            engine.Remove((T)node);
        }
    }

    class NodeEngineWrapper<T, U, V>: SingleNodeEngine<T>   where T : class, INode 
                                                            where U : class, INode 
                                                            where V : class, INode
    {
        INodeEngine<T, U, V> engine;

        public NodeEngineWrapper(INodeEngine<T, U, V> engine)
        {
            this.engine = engine;
        }

        protected override void Add(T node)
        {
            engine.Add((T)node);
        }

        protected override void Remove(T node)
        {
            engine.Remove((T)node);
        }
    }
}