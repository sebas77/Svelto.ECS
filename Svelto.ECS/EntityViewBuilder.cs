using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.Utilities;

namespace Svelto.ECS
{
    public class EntityViewBuilder<EntityViewType> : IEntityViewBuilder where EntityViewType : IEntityData
    {
        public void BuildEntityViewAndAddToList(ref ITypeSafeList list, EGID entityID, object[] implementors)
        {
            if (list == null)
                list = new TypeSafeFasterListForECSForClasses<EntityViewType>();

            var castedList = list as TypeSafeFasterListForECSForClasses<EntityViewType>;

            var lentityView = EntityView<EntityViewType>.BuildEntityView(entityID);

            castedList.Add(lentityView);

            var entityView = lentityView;

            this.FillEntityView(ref entityView
                                             , entityViewBlazingFastReflection
                                               , implementors
                                               , DESCRIPTOR_NAME);
        }

        public ITypeSafeList Preallocate(ref ITypeSafeList list, int size)
        {
            if (list == null)
                list = new TypeSafeFasterListForECSForClasses<EntityViewType>(size);
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
            var fromCastedList = fromSafeList as TypeSafeFasterListForECSForClasses<EntityViewType>;
            var toCastedList = toSafeList as TypeSafeFasterListForECSForClasses<EntityViewType>;

            toCastedList.Add(fromCastedList[fromCastedList.GetIndexFromID(entityID)]);
        }

        public bool isQueryiableEntityView
        {
            get { return true; }
        }

        FasterList<KeyValuePair<Type, CastedAction<EntityViewType>>> entityViewBlazingFastReflection
        {
            get { return EntityView<EntityViewType>.FieldCache<EntityViewType>.list; }
        }

        static readonly Type ENTITY_VIEW_TYPE = typeof(EntityViewType);
        static string DESCRIPTOR_NAME = ENTITY_VIEW_TYPE.ToString();
    }
}