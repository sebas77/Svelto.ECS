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

            var castedDic = list as TypeSafeDictionary<EntityViewType>;
            
            castedDic.Add(entityID.entityID, _initializer);
        }

        public ITypeSafeDictionary Preallocate(ref ITypeSafeDictionary list, int size)
        {
            if (list == null)
                list = new TypeSafeDictionary<EntityViewType>(size);
            else
                list.AddCapacity(size);

            return list;
        }

        public Type GetEntityType()
        {
            return ENTITY_VIEW_TYPE;
        }

        public void MoveEntityView(EGID entityID, ITypeSafeDictionary fromSafeList, ITypeSafeDictionary toSafeList)
        {
            var fromCastedDic = fromSafeList as TypeSafeDictionary<EntityViewType>;
            var toCastedDic   = toSafeList as TypeSafeDictionary<EntityViewType>;

            toCastedDic.Add(entityID.entityID, fromCastedDic[entityID.entityID]);
            fromCastedDic.Remove(entityID.entityID);
        }

        static readonly Type ENTITY_VIEW_TYPE = typeof(EntityViewType);
        internal EntityViewType _initializer;
   }    
}