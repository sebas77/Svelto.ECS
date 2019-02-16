using System;
using System.Collections.Generic;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        class DoubleBufferedEntitiesToAdd<T> where T : FasterDictionary<int, Dictionary<Type, ITypeSafeDictionary>>, new()
        {
            readonly T _entityViewsToAddBufferA = new T();
            readonly T _entityViewsToAddBufferB = new T();

            internal DoubleBufferedEntitiesToAdd()
            {
                other = _entityViewsToAddBufferA;
                current = _entityViewsToAddBufferB;
            }

            internal T other;
            internal T current;
            
            internal void Swap()
            {
                var toSwap = other;
                other = current;
                current = toSwap;
            }

            public void ClearOther()
            {
                foreach (var item in other)
                {
                    foreach (var subitem in item.Value)
                    {
                        subitem.Value.Clear();
                    }
                    
                    item.Value.Clear();
                }
                
                other.Clear();
            }
        }
    }
}