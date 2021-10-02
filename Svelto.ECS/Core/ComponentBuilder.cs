using System;
using System.Collections.Generic;
using System.Reflection;
using DBC.ECS;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Internal;
using Svelto.Utilities;

namespace Svelto.ECS
{
    struct ComponentBuilderComparer : IEqualityComparer<IComponentBuilder>
    {
        public bool Equals(IComponentBuilder x, IComponentBuilder y)
        {
            return x.GetEntityComponentType() == y.GetEntityComponentType();
        }

        public int GetHashCode(IComponentBuilder obj)
        {
            return obj.GetEntityComponentType().GetHashCode();
        }
    }
    
    public class ComponentBuilder<T> : IComponentBuilder
        where T : struct, IEntityComponent
    {
        internal static readonly Type ENTITY_COMPONENT_TYPE;
        public static readonly   bool HAS_EGID;
        internal static readonly bool IS_ENTITY_VIEW_COMPONENT;

        static readonly T      DEFAULT_IT;
        static readonly string ENTITY_COMPONENT_NAME;
        static readonly bool   IS_UNMANAGED;
        public static   bool   HAS_REFERENCE;

        static ComponentBuilder()
        {
            ENTITY_COMPONENT_TYPE    = typeof(T);
            DEFAULT_IT               = default;
            IS_ENTITY_VIEW_COMPONENT = typeof(IEntityViewComponent).IsAssignableFrom(ENTITY_COMPONENT_TYPE);
            HAS_EGID                 = typeof(INeedEGID).IsAssignableFrom(ENTITY_COMPONENT_TYPE);
            HAS_REFERENCE            = typeof(INeedEntityReference).IsAssignableFrom(ENTITY_COMPONENT_TYPE);
            ENTITY_COMPONENT_NAME    = ENTITY_COMPONENT_TYPE.ToString();
            IS_UNMANAGED             = ENTITY_COMPONENT_TYPE.IsUnmanagedEx();

            if (IS_UNMANAGED)
                EntityComponentIDMap.Register<T>(new Filler<T>());

            SetEGIDWithoutBoxing<T>.Warmup();

            ComponentBuilderUtilities.CheckFields(ENTITY_COMPONENT_TYPE, IS_ENTITY_VIEW_COMPONENT);

            if (IS_ENTITY_VIEW_COMPONENT)
            {
                EntityViewComponentCache.InitCache();
            }
            else
            {
                if (ENTITY_COMPONENT_TYPE != ComponentBuilderUtilities.ENTITY_INFO_COMPONENT
                 && ENTITY_COMPONENT_TYPE.IsUnmanagedEx() == false)
                    throw new Exception(
                        $"Entity Component check failed, unexpected struct type (must be unmanaged) {ENTITY_COMPONENT_TYPE}");
            }
        }

        public ComponentBuilder() { _initializer = DEFAULT_IT; }

        public ComponentBuilder(in T initializer) : this() { _initializer = initializer; }

        public bool isUnmanaged => IS_UNMANAGED;

        public void BuildEntityAndAddToList(ITypeSafeDictionary dictionary, EGID egid, IEnumerable<object> implementors)
        {
            var castedDic = dictionary as ITypeSafeDictionary<T>;

            T entityComponent = default;

            if (IS_ENTITY_VIEW_COMPONENT)
            {
                Check.Require(castedDic.ContainsKey(egid.entityID) == false
                            , $"building an entity with already used entity id! id: '{(ulong) egid}', {ENTITY_COMPONENT_NAME}");

                this.SetEntityViewComponentImplementors(ref entityComponent, EntityViewComponentCache.cachedFields
                                                      , implementors, EntityViewComponentCache.implementorsByType
                                                      , EntityViewComponentCache.cachedTypes);

                castedDic.Add(egid.entityID, entityComponent);
            }
            else
            {
                Check.Require(!castedDic.ContainsKey(egid.entityID)
                            , $"building an entity with already used entity id! id: '{egid.entityID}'");

                castedDic.Add(egid.entityID, _initializer);
            }
        }

        void IComponentBuilder.Preallocate(ITypeSafeDictionary dictionary, uint size) { Preallocate(dictionary, size); }

        public ITypeSafeDictionary CreateDictionary(uint size) { return TypeSafeDictionaryFactory<T>.Create(size); }

        public Type GetEntityComponentType() { return ENTITY_COMPONENT_TYPE; }

        public override int GetHashCode() { return _initializer.GetHashCode(); }

        static void Preallocate(ITypeSafeDictionary dictionary, uint size) { dictionary.ResizeTo(size); }

        readonly T _initializer;

        /// <summary>
        ///     Note: this static class will hold forever the references of the entities implementors. These references
        ///     are not even cleared when the engines root is destroyed, as they are static references.
        ///     It must be seen as an application-wide cache system. Honestly, I am not sure if this may cause leaking
        ///     issues in some kind of applications. To remember.
        /// </summary>
        static class EntityViewComponentCache
        {
            internal static readonly FasterList<KeyValuePair<Type, FastInvokeActionCast<T>>> cachedFields;
            internal static readonly Dictionary<Type, Type[]>                                cachedTypes;
#if DEBUG && !PROFILE_SVELTO
            internal static readonly Dictionary<Type, ECSTuple<object, int>> implementorsByType;
#else
            internal static readonly Dictionary<Type, object> implementorsByType;
#endif
            static EntityViewComponentCache()
            {
                cachedFields = new FasterList<KeyValuePair<Type, FastInvokeActionCast<T>>>();

                var type   = typeof(T);
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

                for (var i = fields.Length - 1; i >= 0; --i)
                {
                    var field = fields[i];
                    if (field.FieldType.IsInterface == true)
                    {
                        var setter = FastInvoke<T>.MakeSetter(field);

                        //for each interface, cache the setter for this type 
                        cachedFields.Add(new KeyValuePair<Type, FastInvokeActionCast<T>>(field.FieldType, setter));
                    }
                }
#if DEBUG && !PROFILE_SVELTO
                if (fields.Length == 0)
                    Console.LogWarning($"No fields found in component {type}. Are you declaring only properties?");
#endif

                cachedTypes = new Dictionary<Type, Type[]>();

#if DEBUG && !PROFILE_SVELTO
                implementorsByType = new Dictionary<Type, ECSTuple<object, int>>();
#else
                implementorsByType = new Dictionary<Type, object>();
#endif
            }

            internal static void InitCache() { }
        }
    }
}