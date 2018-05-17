using System;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public interface IEntityViewBuilder
    {
        void BuildEntityViewAndAddToList(ref ITypeSafeDictionary list, EGID entityID, object[] implementors);
        ITypeSafeDictionary Preallocate(ref ITypeSafeDictionary list, int size);

        Type GetEntityType();
        void MoveEntityView(EGID entityID, ITypeSafeDictionary fromSafeList, ITypeSafeDictionary toSafeList);
    }
}