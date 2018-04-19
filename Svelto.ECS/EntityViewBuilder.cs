using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.Utilities;

namespace Svelto.ECS
{
    public class EntityViewBuilder<EntityViewType> : IEntityViewBuilder where EntityViewType : IEntityData, new()
    {
        public EntityViewBuilder(ref EntityViewType initializer)
        {
            _initializer = initializer;
        }
        
        public EntityViewBuilder()
        {}
        
        public void BuildEntityViewAndAddToList(ref ITypeSafeDictionary list, EGID entityID, object[] implementors)
        {
            if (list == null)
                list = new TypeSafeDictionary<EntityViewType>();

            var castedList = list as TypeSafeDictionary<EntityViewType>;

            if (implementors != null)
            {
                EntityViewType lentityView; 
                    
                EntityView<EntityViewType>.BuildEntityView(entityID, out lentityView);

                this.FillEntityView(ref lentityView
                                  , entityViewBlazingFastReflection
                                  , implementors
                                  , DESCRIPTOR_NAME);
                
                castedList.Add(entityID.GID, ref lentityView);
            }
            else
            {
                DBC.Check.Require(_initializer != null, "Implementors not found on a EntityView instance");
                   
                _initializer.ID = entityID;
                
                castedList.Add(entityID.GID, ref _initializer);
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

        public Type GetEntityViewType()
        {
            return ENTITY_VIEW_TYPE;
        }

        public void MoveEntityView(EGID entityID, ITypeSafeDictionary fromSafeList, ITypeSafeDictionary toSafeList)
        {
            var fromCastedList = fromSafeList as TypeSafeDictionary<EntityViewType>;
            var toCastedList = toSafeList as TypeSafeDictionary<EntityViewType>;

            toCastedList.Add(entityID.GID, fromCastedList[entityID.GID]);
            fromCastedList.Remove(entityID.GID);
        }

        FasterList<KeyValuePair<Type, CastedAction<EntityViewType>>> entityViewBlazingFastReflection
        {
            get { return EntityView<EntityViewType>.FieldCache.list; }
        }
        
        internal EntityViewType _initializer;
        
        static readonly Type ENTITY_VIEW_TYPE = typeof(EntityViewType);
        static string DESCRIPTOR_NAME = ENTITY_VIEW_TYPE.ToString();
    }
}