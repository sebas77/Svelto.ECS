using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
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

    public class ComponentBuilder<T> : IComponentBuilder where T : struct, _IInternalEntityComponent
    {
        internal static readonly Type ENTITY_COMPONENT_TYPE;
        internal static readonly bool IS_ENTITY_VIEW_COMPONENT;

        static readonly string ENTITY_COMPONENT_NAME;
        static readonly bool   IS_UNMANAGED;
#if SLOW_SVELTO_SUBMISSION            
        public static readonly bool HAS_EGID;
        public static readonly bool HAS_REFERENCE;
#endif

        static ComponentBuilder()
        {
            ENTITY_COMPONENT_TYPE = typeof(T);
            IS_ENTITY_VIEW_COMPONENT = typeof(IEntityViewComponent).IsAssignableFrom(ENTITY_COMPONENT_TYPE);
#if SLOW_SVELTO_SUBMISSION            
            HAS_EGID = typeof(INeedEGID).IsAssignableFrom(ENTITY_COMPONENT_TYPE);
            HAS_REFERENCE = typeof(INeedEntityReference).IsAssignableFrom(ENTITY_COMPONENT_TYPE);
            
            SetEGIDWithoutBoxing<T>.Warmup();
#endif
            ENTITY_COMPONENT_NAME = ENTITY_COMPONENT_TYPE.ToString();
            IS_UNMANAGED = TypeCache<T>.isUnmanaged; //attention this is important as it serves as warm up for Type<T>
#if UNITY_NATIVE
            if (IS_UNMANAGED)
                EntityComponentIDMap.Register<T>(new Filler<T>());
#endif
            ComponentTypeID<T>.Init();
            ComponentBuilderUtilities.CheckFields(ENTITY_COMPONENT_TYPE, IS_ENTITY_VIEW_COMPONENT);

            if (IS_ENTITY_VIEW_COMPONENT)
            {
                EntityViewComponentCache.InitCache();
            }
            else
            {
                if (ENTITY_COMPONENT_TYPE != ComponentBuilderUtilities.ENTITY_INFO_COMPONENT &&
                    TypeCache<T>.isUnmanaged == false)
                    throw new Exception(
                        $"Entity Component check failed, unexpected struct type (must be unmanaged) {ENTITY_COMPONENT_TYPE}");
            }
        }

        public ComponentBuilder()
        {
            _initializer = default;
        }

        public ComponentBuilder(in T initializer) : this()
        {
            _initializer = initializer;
        }

        public bool isUnmanaged => IS_UNMANAGED;
        public ComponentID getComponentID => ComponentTypeID<T>.id;

        static readonly ThreadLocal<EntityViewComponentCache> _localCache = new ThreadLocal<EntityViewComponentCache>(() => new EntityViewComponentCache());

        public void BuildEntityAndAddToList(ITypeSafeDictionary dictionary, EGID egid, IEnumerable<object> implementors)
        {
            var castedDic = dictionary as ITypeSafeDictionary<T>;

            if (IS_ENTITY_VIEW_COMPONENT)
            {
                T entityComponent = default;
                
                Check.Require(castedDic.ContainsKey(egid.entityID) == false,
                    $"building an entity with already used entity id! id: '{(ulong)egid}', {ENTITY_COMPONENT_NAME}");

                this.SetEntityViewComponentImplementors(ref entityComponent, implementors, _localCache.Value);

                castedDic.Add(egid.entityID, entityComponent);
            }
            else
            {
                Check.Require(!castedDic.ContainsKey(egid.entityID),
                    $"building an entity with already used entity id! id: '{egid.entityID}'");

                castedDic.Add(egid.entityID, _initializer);
            }
        }

        void IComponentBuilder.Preallocate(ITypeSafeDictionary dictionary, uint size)
        {
            Preallocate(dictionary, size);
        }

        public ITypeSafeDictionary CreateDictionary(uint size)
        {
            return TypeSafeDictionaryFactory<T>.Create(size);
        }

        public Type GetEntityComponentType()
        {
            return ENTITY_COMPONENT_TYPE;
        }

        public override int GetHashCode()
        {
            return _initializer.GetHashCode();
        }

        static void Preallocate(ITypeSafeDictionary dictionary, uint size)
        {
            dictionary.EnsureCapacity(size);
        }

        readonly T _initializer;

        internal class EntityViewComponentCache
        {
            internal readonly FasterList<KeyValuePair<Type, FastInvokeActionCast<T>>> cachedFields;
            internal readonly Dictionary<Type, Type[]>                                cachedTypes;
            
            //this is just a local static cache that is cleared after every use
#if DEBUG && !PROFILE_SVELTO
            internal readonly Dictionary<Type, ECSTuple<object, int>> implementorsByType;
#else
            internal readonly Dictionary<Type, object> implementorsByType;
#endif
            internal EntityViewComponentCache()
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

            internal static void InitCache()
            {
            }
        }
    }
}