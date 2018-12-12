using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.Utilities;

namespace Svelto.ECS
{
    public partial class EntityBuilder<T> : IEntityBuilder where T : IEntityStruct, new()
    {
        public EntityBuilder()
        {
            _initializer = DEFAULT_IT;

            CheckFields(ENTITY_VIEW_TYPE, NEEDS_REFLECTION, true);

            if (NEEDS_REFLECTION == true)
                EntityView<T>.InitCache();
        }

        public void BuildEntityAndAddToList(ref ITypeSafeDictionary dictionary, EGID entityID, object[] implementors)
        {
            if (dictionary == null)
                dictionary = new TypeSafeDictionary<T>();

            var castedDic = dictionary as TypeSafeDictionary<T>;

            if (NEEDS_REFLECTION == true)
            {
                DBC.ECS.Check.Require(implementors != null, "Implementors not found while building an EntityView");
                DBC.ECS.Check.Require(castedDic.ContainsKey(entityID.entityID) == false,
                                      "building an entity with already used entity id! id".FastConcat(entityID).FastConcat(" ", ENTITY_VIEW_NAME));

                T entityView;
                EntityView<T>.BuildEntityView(entityID, out entityView);

                this.FillEntityView(ref entityView
                                  , entityViewBlazingFastReflection
                                  , implementors);
                
                castedDic.Add(entityID.entityID, ref entityView);
            }
            else
            {
                _initializer.ID = entityID;
                
                castedDic.Add(entityID.entityID, _initializer);
            }
        }

        ITypeSafeDictionary IEntityBuilder.Preallocate(ref ITypeSafeDictionary dictionary, int size)
        {
            return Preallocate(ref dictionary, size);
        }

        public static ITypeSafeDictionary Preallocate(ref ITypeSafeDictionary dictionary, int size)
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

        static FasterList<KeyValuePair<Type, ActionCast<T>>> entityViewBlazingFastReflection
        {
            get { return EntityView<T>.cachedFields; }
        }
        
        static readonly Type   ENTITY_VIEW_TYPE = typeof(T);
        static readonly T      DEFAULT_IT       = default(T);
        static readonly bool   NEEDS_REFLECTION = typeof(IEntityViewStruct).IsAssignableFrom(typeof(T));
        static readonly string ENTITY_VIEW_NAME = ENTITY_VIEW_TYPE.ToString();

        internal T _initializer;
        
    }
}