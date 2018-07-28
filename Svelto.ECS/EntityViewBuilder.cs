using System;
using System.Collections.Generic;
#if DEBUG && !PROFILER 
using System.Reflection;
#endif    
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.Utilities;

namespace Svelto.ECS
{
    public class EntityBuilder<T> : IEntityBuilder where T : IEntityStruct, new()
    {
        public EntityBuilder()
        {
            _initializer = default(T);

#if DEBUG && !PROFILER
            if (needsReflection == false && typeof(T) != typeof(EntityInfoView))
            {
                CheckFields(typeof(T));
            }
#endif
            if (needsReflection == true)
            {
                EntityView<T>.InitCache();
            }
        }
#if DEBUG && !PROFILER
        static void CheckFields(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public |
                                        BindingFlags.Instance);

            for (int i = fields.Length - 1; i >= 0; --i)
            {
                var field = fields[i];

                var fieldFieldType = field.FieldType;
                if (fieldFieldType.IsPrimitive == true || fieldFieldType.IsValueType == true)
                {
                    if (fieldFieldType.IsValueType && !fieldFieldType.IsEnum)
                    {
                        CheckFields(fieldFieldType);
                    }

                    continue;
                }

                throw new EntityStructException(fieldFieldType);
            }
        }
#endif        

        public void BuildEntityViewAndAddToList(ref ITypeSafeDictionary dictionary, EGID entityID, object[] implementors)
        {
            if (dictionary == null)
                dictionary = new TypeSafeDictionary<T>();

            var castedDic = dictionary as TypeSafeDictionary<T>;

            if (needsReflection == true)
            {
                DBC.ECS.Check.Require(implementors != null, "Implementors not found while building an EntityView");

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
        
        static readonly Type ENTITY_VIEW_TYPE = typeof(T);
        static readonly string DESCRIPTOR_NAME = ENTITY_VIEW_TYPE.ToString();
        static readonly bool needsReflection = typeof(IEntityViewStruct).IsAssignableFrom(typeof(T));

        internal T _initializer;
    }

    public class EntityStructException : Exception
    {
        public EntityStructException(Type fieldType):base("EntityStruct must contains only value types! " + fieldType.ToString())
        {}
    }
}