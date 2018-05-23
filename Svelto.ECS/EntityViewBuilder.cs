using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.Utilities;

namespace Svelto.ECS
{
    public class EntityViewBuilder<T> : IEntityViewBuilder where T : IEntityData, new()
    {
        public EntityViewBuilder(ref T initializer)
        {
            _initializer = initializer;
        }
        
        public EntityViewBuilder()
        {
            _initializer = default(T);
        }
        
        public void BuildEntityViewAndAddToList(ref ITypeSafeDictionary dictionary, EGID entityID, object[] implementors)
        {
            if (dictionary == null)
                dictionary = new TypeSafeDictionary<T>();

            var castedDic = dictionary as TypeSafeDictionary<T>;

            if (needsReflection == true)
            {
                DBC.Check.Require(implementors != null, "Implementors not found while building an EntityView");

                T lentityView;
                EntityView<T>.BuildEntityView(entityID, out lentityView);

                this.FillEntityView(ref lentityView
                                  , entityViewBlazingFastReflection
                                  , implementors
                                  , DESCRIPTOR_NAME);

                castedDic.Add(entityID.entityID, ref lentityView);
            }
            else
            {
                _initializer.ID = entityID;
                
                castedDic.Add(entityID.entityID, _initializer);
            }
        }

        public ITypeSafeDictionary Preallocate(ref ITypeSafeDictionary dictionary, int size)
        {
            if (dictionary == null)
                dictionary = new TypeSafeDictionary<T>(size);
            else
                dictionary.AddCapacity(size);

            return dictionary;
        }

        public Type GetEntityType()
        {
            return ENTITY_VIEW_TYPE;
        }

        public void MoveEntityView(EGID entityID, ITypeSafeDictionary fromSafeDic, ITypeSafeDictionary toSafeDic)
        {
            var fromCastedDic = fromSafeDic as TypeSafeDictionary<T>;
            var toCastedDic = toSafeDic as TypeSafeDictionary<T>;

            toCastedDic.Add(entityID.entityID, fromCastedDic[entityID.entityID]);
            fromCastedDic.Remove(entityID.entityID);
        }

        FasterList<KeyValuePair<Type, ActionRef<T>>> entityViewBlazingFastReflection
        {
            get { return EntityView<T>.FieldCache.list; }
        }
        
        static readonly Type ENTITY_VIEW_TYPE = typeof(T);
        static string DESCRIPTOR_NAME = ENTITY_VIEW_TYPE.ToString();

        internal T _initializer;
        readonly bool needsReflection = typeof(IEntityView).IsAssignableFrom(typeof(T));
    }
}