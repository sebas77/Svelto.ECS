using System;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public interface IEntityBuilder
    {
        void BuildEntityViewAndAddToList(ref ITypeSafeDictionary dictionary, EGID entityID, object[] implementors);
        ITypeSafeDictionary Preallocate(ref ITypeSafeDictionary dictionary, int size);

        Type GetEntityType();
        void MoveEntityView(EGID entityID, int toGroupID, ITypeSafeDictionary fromSafeDic, ITypeSafeDictionary toSafeDic);
    }
}