using System;
using System.Collections.Generic;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public interface IComponentBuilder
    {
        void BuildEntityAndAddToList(ITypeSafeDictionary dictionary, EGID egid, IEnumerable<object> implementors);
        void Preallocate(ITypeSafeDictionary dictionary, uint size);
        ITypeSafeDictionary CreateDictionary(uint size);

        Type                GetEntityComponentType();
        bool                isUnmanaged { get; }
        ComponentID getComponentID { get; }
    }
}