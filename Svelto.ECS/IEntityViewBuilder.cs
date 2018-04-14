using System;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public interface IEntityViewBuilder
    {
        void          BuildEntityViewAndAddToList(ref ITypeSafeList list, EGID entityID, object[] implementors);
        ITypeSafeList Preallocate(ref                 ITypeSafeList list, int  size);

        Type GetEntityViewType();
        void MoveEntityView(EGID entityID, ITypeSafeList fromSafeList, ITypeSafeList toSafeList);

        bool isQueryiableEntityView { get; }
    }
}