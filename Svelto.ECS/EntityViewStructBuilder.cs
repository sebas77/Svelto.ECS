using System;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public class EntityViewStructBuilder<EntityViewType> : IEntityViewBuilder where EntityViewType : struct, IEntityData
    {
        public EntityViewStructBuilder(ref EntityViewType initializer)
        {
            _initializer = initializer;
        }
        
        public void BuildEntityViewAndAddToList(ref ITypeSafeList list, EGID entityID, object[] implementors = null)
        {
            _initializer.ID = entityID;
            
            if (list == null)
                list = new TypeSafeFasterListForECSForStructs<EntityViewType>();

            var castedList = list as TypeSafeFasterListForECSForStructs<EntityViewType>;
            
            castedList.Add(_initializer);
        }

        public ITypeSafeList Preallocate(ref ITypeSafeList list, int size)
        {
            if (list == null)
                list = new TypeSafeFasterListForECSForStructs<EntityViewType>(size);
            else
                list.AddCapacity(size);

            return list;
        }

        public Type GetEntityViewType()
        {
            return ENTITY_VIEW_TYPE;
        }

        public void MoveEntityView(EGID entityID, ITypeSafeList fromSafeList, ITypeSafeList toSafeList)
        {
            var fromCastedList = fromSafeList as TypeSafeFasterListForECSForStructs<EntityViewType>;
            var toCastedList = toSafeList as TypeSafeFasterListForECSForStructs<EntityViewType>;

            toCastedList.Add(fromCastedList[fromCastedList.GetIndexFromID(entityID)]);
        }

        public bool isQueryiableEntityView
        {
            get { return false; }
        }

        static readonly Type ENTITY_VIEW_TYPE = typeof(EntityViewType);
        EntityViewType _initializer;
   }    
}