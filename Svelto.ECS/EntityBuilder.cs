#if !DEBUG || PROFILER
#define DISABLE_CHECKS
using System.Diagnostics;
#endif    
using System.Reflection;
using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.Utilities;

namespace Svelto.ECS
{
    public class EntityBuilder<T> : IEntityBuilder where T : IEntityStruct, new()
    {
        public EntityBuilder()
        {
            _initializer = defaultIt;

            CheckFields(ENTITY_VIEW_TYPE);

            if (needsReflection == true)
                EntityView<T>.InitCache();
        }

#if DISABLE_CHECKS        
        [Conditional("_CHECKS_DISABLED")]
#endif        
        static void CheckFields(Type type)
        {
            if (needsReflection == false && ENTITY_VIEW_TYPE != typeof(EntityInfoView))
            {
                var fields = type.GetFields(BindingFlags.Public |
                                            BindingFlags.Instance);

                for (int i = fields.Length - 1; i >= 0; --i)
                {
                    var field = fields[i];

                    var fieldFieldType = field.FieldType;

                    SubCheckFields(fieldFieldType);
                }

                if (type.Assembly == Assembly.GetCallingAssembly() && type != EGIDType)
                {
                    var methods = type.GetMethods(BindingFlags.Public   |
                                                  BindingFlags.Instance | BindingFlags.DeclaredOnly);

                    var properties = type.GetProperties(BindingFlags.Public   |
                                                        BindingFlags.Instance | BindingFlags.DeclaredOnly);

                    if (methods.Length > properties.Length + 1)
                        throw new EntityStructException(type);

                    for (int i = properties.Length - 1; i >= 0; --i)
                    {
                        var propertyInfo = properties[i];

                        var fieldFieldType = propertyInfo.PropertyType;
                        SubCheckFields(fieldFieldType);
                    }
                }
            }
        }

        static void SubCheckFields(Type fieldFieldType)
        {
            if (fieldFieldType.IsPrimitive == true || fieldFieldType.IsValueType == true)
            {
                if (fieldFieldType.IsValueType && !fieldFieldType.IsEnum && fieldFieldType.IsPrimitive == false)
                {
                    CheckFields(fieldFieldType);
                }

                return;
            }

            throw new EntityStructException(fieldFieldType);
        }
   

        public void BuildEntityAndAddToList(ref ITypeSafeDictionary dictionary, EGID entityID, object[] implementors)
        {
            if (dictionary == null)
                dictionary = new TypeSafeDictionary<T>();

            var castedDic = dictionary as TypeSafeDictionary<T>;

            if (needsReflection == true)
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
        static readonly string ENTITY_VIEW_NAME = ENTITY_VIEW_TYPE.ToString();
        static readonly bool   needsReflection  = typeof(IEntityViewStruct).IsAssignableFrom(typeof(T));
        static readonly T      defaultIt        = default(T);
        static readonly Type   EGIDType         = typeof(EGID);

        internal T _initializer;
    }

    public class EntityStructException : Exception
    {
        public EntityStructException(Type fieldType) :
            base("EntityStruct must contains only value types and no public methods! " + fieldType.ToString())
        {}
    }
}