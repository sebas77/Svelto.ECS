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
        
        public EntityViewStructBuilder()
        {
            _initializer = default(EntityViewType);
        }
        
        public void BuildEntityViewAndAddToList(ref ITypeSafeDictionary list, EGID entityID, object[] implementors = null)
        {
            _initializer.ID = entityID;
            
            if (list == null)
                list = new TypeSafeDictionary<EntityViewType>();

            var castedList = list as TypeSafeDictionary<EntityViewType>;
            
            castedList.Add(entityID.GID, _initializer);
        }

        public ITypeSafeDictionary Preallocate(ref ITypeSafeDictionary list, int size)
        {
            if (list == null)
                list = new TypeSafeDictionary<EntityViewType>(size);
            else
                list.AddCapacity(size);

            return list;
        }

        public Type GetEntityViewType()
        {
            return ENTITY_VIEW_TYPE;
        }

        public void MoveEntityView(EGID entityID, ITypeSafeDictionary fromSafeList, ITypeSafeDictionary toSafeList)
        {
            var fromCastedList = fromSafeList as TypeSafeDictionary<EntityViewType>;
            var toCastedList   = toSafeList as TypeSafeDictionary<EntityViewType>;

            toCastedList.Add(entityID.GID, fromCastedList[entityID.GID]);
            fromCastedList.Remove(entityID.GID);
        }

        static readonly Type ENTITY_VIEW_TYPE = typeof(EntityViewType);
        internal EntityViewType _initializer;
   }    
}