using System;
using System.Collections.Generic;


namespace Svelto.ES
{
    public sealed class EnginesRoot: INodeEnginesRoot
	{
		public EnginesRoot()
		{
            _nodeEngines = new Dictionary<Type, List<INodeEngine>>();
        }

        public void AddEngine(IEngine engine)
        {
            if (engine is INodeEngine)
                AddNodeEngine(engine as INodeEngine);
        }

        public void Add(INode node)
        {
            Type nodeType = node.GetType();
            
            List<INodeEngine> value;
            if (_nodeEngines.TryGetValue(nodeType, out value))
                for (int j = 0; j < value.Count; j++)
                   value[j].Add(node);
        }

        public void Remove(INode node)
        {
            Type nodeType = node.GetType();

            List<INodeEngine> value;
            if (_nodeEngines.TryGetValue(nodeType, out value))
                for (int j = 0; j < value.Count; j++)
                    value[j].Remove(node);
        }

        void AddNodeEngine(INodeEngine engine)
        {
            AddEngine(engine, engine.AcceptedNodes(), _nodeEngines);
        }

        void AddEngine<T>(T engine, Type[] types, Dictionary<Type, List<T>> engines)
        {
            for (int i = 0; i < types.Length; i++)
            {
                List<T> value;

                var type = types[i];

                if (engines.TryGetValue(type, out value) == false)
                {
                    List<T> list = new List<T>();

                    list.Add(engine);

                    engines.Add(type, list);
                }
                else
                    value.Add(engine);
            }
        }

        Dictionary<Type, List<INodeEngine>>         _nodeEngines;
    }
}

