using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Internal;
using Svelto.Utilities;

namespace Svelto.ECS
{
    public class EntityBuilder<T> : IEntityBuilder where T : struct, IEntityStruct
    {
        public EntityBuilder()
        {
            _initializer = DEFAULT_IT;

            EntityBuilderUtilities.CheckFields(ENTITY_VIEW_TYPE, NEEDS_REFLECTION);

            if (NEEDS_REFLECTION)
                EntityView<T>.InitCache();
        }

        public void BuildEntityAndAddToList(ref ITypeSafeDictionary dictionary, EGID entityID, object[] implementors)
        {
            if (dictionary == null)
                dictionary = new TypeSafeDictionary<T>();

            var castedDic = dictionary as TypeSafeDictionary<T>;

            if (NEEDS_REFLECTION)
            {
                DBC.ECS.Check.Require(implementors != null, "Implementors not found while building an EntityView");
                DBC.ECS.Check.Require(castedDic.ContainsKey(entityID.entityID) == false,
                              "building an entity with already used entity id! id: ".FastConcat((ulong) entityID)
                                 .FastConcat(" ", ENTITY_VIEW_NAME));

                EntityView<T>.BuildEntityView(out var entityView);

                this.FillEntityView(ref entityView, entityViewBlazingFastReflection, implementors, implementorsByType,
                                    cachedTypes);
                
                castedDic.Add(entityID.entityID, ref entityView);
            }
            else
            {
                castedDic.Add(entityID.entityID, _initializer);
            }
        }

        ITypeSafeDictionary IEntityBuilder.Preallocate(ref ITypeSafeDictionary dictionary, uint size)
        {
            return Preallocate(ref dictionary, size);
        }

        static ITypeSafeDictionary Preallocate(ref ITypeSafeDictionary dictionary, uint size)
        {
            if (dictionary == null)
                dictionary = new TypeSafeDictionary<T>(size);
            else
                dictionary.SetCapacity(size);

            return dictionary;
        }

        public Type GetEntityType() { return ENTITY_VIEW_TYPE; }

#if DEBUG && !PROFILER
        readonly Dictionary<Type, ECSTuple<object, int>> implementorsByType =
            new Dictionary<Type, ECSTuple<object, int>>();
#else
        readonly Dictionary<Type, object> implementorsByType = new Dictionary<Type, object>();
#endif

        //this is used to avoid newing a dictionary every time, but it's used locally only and it's cleared for each use
        readonly Dictionary<Type, Type[]> cachedTypes = new Dictionary<Type, Type[]>();

        static FasterList<KeyValuePair<Type, ActionCast<T>>> entityViewBlazingFastReflection =>
            EntityView<T>.cachedFields;

        internal static readonly Type   ENTITY_VIEW_TYPE    = typeof(T);
        static readonly T      DEFAULT_IT          = default;
        static readonly bool   NEEDS_REFLECTION    = typeof(IEntityViewStruct).IsAssignableFrom(typeof(T));
        static readonly string ENTITY_VIEW_NAME    = ENTITY_VIEW_TYPE.ToString();
        internal static readonly bool HAS_EGID = typeof(INeedEGID).IsAssignableFrom(ENTITY_VIEW_TYPE);

        internal T _initializer;
    }
}