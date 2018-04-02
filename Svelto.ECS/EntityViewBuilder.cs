using System;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public interface IEntityViewBuilder
    {
        void BuildEntityViewAndAddToList(ref ITypeSafeList list, EGID entityID, out IEntityView entityView);
        ITypeSafeList Preallocate(ref ITypeSafeList list, int size);

        Type GetEntityViewType();
        void MoveEntityView(EGID entityID, ITypeSafeList fromSafeList, ITypeSafeList toSafeList);
        bool mustBeFilled { get; }
        bool isQueryiableEntityView { get; }
    }

    public class EntityViewBuilder<EntityViewType> : IEntityViewBuilder where EntityViewType : EntityView, new()
    {
        public void BuildEntityViewAndAddToList(ref ITypeSafeList list, EGID entityID, out IEntityView entityView)
        {
            if (list == null)
                list = new TypeSafeFasterListForECSForClasses<EntityViewType>();

            var castedList = list as TypeSafeFasterListForECSForClasses<EntityViewType>;

            var lentityView = EntityView<EntityViewType>.BuildEntityView(entityID);

            castedList.Add(lentityView);

            entityView = lentityView;
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

        public bool mustBeFilled
        {
            get { return true; }
        }
        
        public bool isQueryiableEntityView
        {
            get { return true; }
        }

        public static readonly Type ENTITY_VIEW_TYPE = typeof(EntityViewType);
    }

    public class EntityViewStructBuilder<EntityViewType> : IEntityViewBuilder where EntityViewType : struct, IEntityStruct
    {
        public EntityViewStructBuilder()
        {}
        
        public EntityViewStructBuilder(ref EntityViewType initializer)
        {
            _initializer = initializer;
        }
        
        public void BuildEntityViewAndAddToList(ref ITypeSafeList list, EGID entityID, out IEntityView entityView)
        {
            _initializer.ID = entityID;
            
            if (list == null)
                list = new TypeSafeFasterListForECSForStructs<EntityViewType>();

            var castedList = list as TypeSafeFasterListForECSForStructs<EntityViewType>;
            
            castedList.Add(_initializer);

            entityView = null;
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

        public bool mustBeFilled
        {
            get { return false; }
        }

        public bool isQueryiableEntityView
        {
            get { return false; }
        }

        public static readonly Type ENTITY_VIEW_TYPE = typeof(EntityViewType);
        EntityViewType _initializer;
   }    
}