using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.Utilities;

namespace Svelto.ECS
{
    public class EntityViewBuilder<EntityViewType> : IEntityViewBuilder where EntityViewType : IEntityData, new()
    {
        public void BuildEntityViewAndAddToList(ref ITypeSafeDictionary list, EGID entityID, object[] implementors)
        {
            if (list == null)
                list = new TypeSafeDictionary<EntityViewType>();

            var castedDic = list as TypeSafeDictionary<EntityViewType>;

            DBC.Check.Require(implementors != null, "Implementors not found while building an EntityView");
            {
                EntityViewType lentityView; 
                    
                EntityView<EntityViewType>.BuildEntityView(entityID, out lentityView);

                this.FillEntityView(ref lentityView
                                  , entityViewBlazingFastReflection
                                  , implementors
                                  , DESCRIPTOR_NAME);
                
                castedDic.Add(entityID.entityID, ref lentityView);
            }
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
            var toCastedDic = toSafeList as TypeSafeDictionary<EntityViewType>;

            toCastedDic.Add(entityID.entityID, fromCastedDic[entityID.entityID]);
            fromCastedDic.Remove(entityID.entityID);
        }

        FasterList<KeyValuePair<Type, ActionRef<EntityViewType>>> entityViewBlazingFastReflection
        {
            get { return EntityView<EntityViewType>.FieldCache.list; }
        }
        
        static readonly Type ENTITY_VIEW_TYPE = typeof(EntityViewType);
        static string DESCRIPTOR_NAME = ENTITY_VIEW_TYPE.ToString();
    }
}