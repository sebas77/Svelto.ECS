using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Internal;
using Svelto.Utilities;

namespace Svelto.ECS
{
    public class EntityBuilder<T> : IEntityBuilder where T : struct, IEntityStruct
    {
        static class EntityView
        {
            internal static readonly FasterList<KeyValuePair<Type, ActionCast<T>>> cachedFields;
            internal static readonly Dictionary<Type, Type[]>                      cachedTypes;
#if DEBUG && !PROFILER
            internal static readonly Dictionary<Type, ECSTuple<object, int>> implementorsByType;
#else
            internal static readonly Dictionary<Type, object> implementorsByType;
#endif
            static EntityView()
            {
                cachedFields = new FasterList<KeyValuePair<Type, ActionCast<T>>>();

                var type = typeof(T);

                var fields = type.GetFields(BindingFlags.Public |
                                            BindingFlags.Instance);

                for (var i = fields.Length - 1; i >= 0; --i)
                {
                    var field = fields[i];

                    var setter = FastInvoke<T>.MakeSetter(field);

                    cachedFields.Add(new KeyValuePair<Type, ActionCast<T>>(field.FieldType, setter));
                }

                cachedTypes = new Dictionary<Type, Type[]>();

#if DEBUG && !PROFILER
                implementorsByType = new Dictionary<Type, ECSTuple<object, int>>();
#else
                implementorsByType = new Dictionary<Type, object>();
#endif
            }

            internal static void InitCache()
            {}

            internal static void BuildEntityView(out T entityView)
            {
                entityView = new T();
            }
        }

        public EntityBuilder()
        {
            _initializer = DEFAULT_IT;

            EntityBuilderUtilities.CheckFields(ENTITY_VIEW_TYPE, NEEDS_REFLECTION);

            if (NEEDS_REFLECTION)
                EntityView.InitCache();
        }

        public EntityBuilder(in T initializer) : this()
        {
            _initializer = initializer;
        }

        public void BuildEntityAndAddToList(ref ITypeSafeDictionary dictionary, EGID egid,
            IEnumerable<object> implementors)
        {
            if (dictionary == null)
                dictionary = new TypeSafeDictionary<T>();

            var castedDic = dictionary as TypeSafeDictionary<T>;

            if (NEEDS_REFLECTION)
            {
                DBC.ECS.Check.Require(implementors != null,
                    $"Implementors not found while building an EntityView `{typeof(T)}`");
                DBC.ECS.Check.Require(castedDic.ContainsKey(egid.entityID) == false,
                    $"building an entity with already used entity id! id: '{(ulong) egid}', {ENTITY_VIEW_NAME}");

                EntityView.BuildEntityView(out var entityView);

                this.FillEntityView(ref entityView, entityViewBlazingFastReflection, implementors,
                    EntityView.implementorsByType, EntityView.cachedTypes);

                castedDic.Add(egid.entityID, entityView);
            }
            else
            {
                DBC.ECS.Check.Require(!castedDic.ContainsKey(egid.entityID),
                    $"building an entity with already used entity id! id: '{egid.entityID}'");

                castedDic.Add(egid.entityID, _initializer);
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

        public Type GetEntityType()
        {
            return ENTITY_VIEW_TYPE;
        }

        static EntityBuilder()
        {
            ENTITY_VIEW_TYPE = typeof(T);
            DEFAULT_IT = default;
            NEEDS_REFLECTION = typeof(IEntityViewStruct).IsAssignableFrom(ENTITY_VIEW_TYPE);
            HAS_EGID = typeof(INeedEGID).IsAssignableFrom(ENTITY_VIEW_TYPE);
            ENTITY_VIEW_NAME = ENTITY_VIEW_TYPE.ToString();
            SetEGIDWithoutBoxing<T>.Warmup();
        }

        readonly T                        _initializer;

        static FasterList<KeyValuePair<Type, ActionCast<T>>> entityViewBlazingFastReflection =>
            EntityView.cachedFields;

        internal static readonly Type ENTITY_VIEW_TYPE;
        public static readonly bool HAS_EGID;

        static readonly T      DEFAULT_IT;
        static readonly bool   NEEDS_REFLECTION;
        static readonly string ENTITY_VIEW_NAME;
    }
}