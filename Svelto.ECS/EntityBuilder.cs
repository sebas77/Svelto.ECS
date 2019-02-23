using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
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
                                      "building an entity with already used entity id! id: ".FastConcat((long)entityID).FastConcat(" ", ENTITY_VIEW_NAME));

                T entityView;
                EntityView<T>.BuildEntityView(entityID, out entityView);

                this.FillEntityView(ref entityView, entityViewBlazingFastReflection, implementors, implementorsByType, 
                                    cachedTypes);
                
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
        
#if DEBUG && !PROFILER
        readonly Dictionary<Type, ECSTuple<object, int>> implementorsByType = new Dictionary<Type, ECSTuple<object, int>>();
#else
        readonly Dictionary<Type, object> implementorsByType = new Dictionary<Type, object>();
#endif
        
        //this is used to avoid newing a dictionary every time, but it's used locally only and it's cleared for each use
        readonly Dictionary<Type, Type[]> cachedTypes = new Dictionary<Type, Type[]>();

        static FasterList<KeyValuePair<Type, ActionCast<T>>> entityViewBlazingFastReflection
        {
            get { return EntityView<T>.cachedFields; }
        }
        
        static readonly Type   ENTITY_VIEW_TYPE = typeof(T);
        static readonly T      DEFAULT_IT       = default(T);
        static readonly Type   ENTITYINFOVIEW_TYPE = typeof(EntityInfoView);
        static readonly bool   NEEDS_REFLECTION = typeof(IEntityViewStruct).IsAssignableFrom(typeof(T));
        static readonly string ENTITY_VIEW_NAME = ENTITY_VIEW_TYPE.ToString();

        internal T _initializer;
        
    }
}