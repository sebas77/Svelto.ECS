#if !DEBUG || PROFILE_SVELTO
#define DISABLE_CHECKS
using System.Diagnostics;
#endif
using System;
using System.Reflection;

namespace Svelto.ECS
{
    internal static class ComponentBuilderUtilities
    {
        const string MSG = "Entity Structs field and Entity View Struct components must hold value types.";


#if DISABLE_CHECKS
        [Conditional("_CHECKS_DISABLED")]
#endif
        public static void CheckFields(Type entityComponentType, bool needsReflection, bool isStringAllowed = false)
        {
            if (entityComponentType == ENTITY_STRUCT_INFO_VIEW ||
                entityComponentType == EGIDType ||
                entityComponentType == EXCLUSIVEGROUPSTRUCTTYPE ||
                entityComponentType == SERIALIZABLE_ENTITY_STRUCT)
            {
                return;
            }

            if (needsReflection == false)
            {
                if (entityComponentType.IsClass)
                {
                    throw new EntityComponentException("EntityComponents must be structs.", entityComponentType);
                }

                FieldInfo[] fields = entityComponentType.GetFields(BindingFlags.Public | BindingFlags.Instance);

                for (var i = fields.Length - 1; i >= 0; --i)
                {
                    FieldInfo fieldInfo = fields[i];
                    Type fieldType = fieldInfo.FieldType;

                    SubCheckFields(fieldType, entityComponentType, isStringAllowed);
                }
            }
            else
            {
                FieldInfo[] fields = entityComponentType.GetFields(BindingFlags.Public | BindingFlags.Instance);

                if (fields.Length < 1)
                {
                    ProcessError("No valid fields found in Entity View Struct", entityComponentType);
                }

                for (int i = fields.Length - 1; i >= 0; --i)
                {
                    FieldInfo fieldInfo = fields[i];

                    if (fieldInfo.FieldType.IsInterfaceEx() == false && fieldInfo.FieldType.IsValueTypeEx() == false)
                    {
                        ProcessError("Entity View Structs must hold only public interfaces or value type fields.",
                            entityComponentType);
                    }

                    PropertyInfo[] properties = fieldInfo.FieldType.GetProperties(
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                    for (int j = properties.Length - 1; j >= 0; --j)
                    {
                        if (properties[j].PropertyType.IsGenericType)
                        {
                            Type genericTypeDefinition = properties[j].PropertyType.GetGenericTypeDefinition();
                            if (genericTypeDefinition == DISPATCHONSETTYPE ||
                                genericTypeDefinition == DISPATCHONCHANGETYPE)
                            {
                                continue;
                            }
                        }

                        Type propertyType = properties[j].PropertyType;
                        if (propertyType != STRINGTYPE)
                        {
                            //for EntityComponentStructs, component fields that are structs that hold strings
                            //are allowed
                            SubCheckFields(propertyType, entityComponentType, isStringAllowed: true);
                        }
                    }
                }
            }
        }

        static void SubCheckFields(Type fieldType, Type entityComponentType, bool isStringAllowed = false)
        {
            if (fieldType.IsPrimitive || fieldType.IsValueType || (isStringAllowed == true && fieldType == STRINGTYPE))
            {
                if (fieldType.IsValueType && !fieldType.IsEnum && fieldType.IsPrimitive == false)
                {
                    CheckFields(fieldType, false, isStringAllowed);
                }

                return;
            }
            
            ProcessError(MSG, entityComponentType, fieldType);
        }

        static void ProcessError(string message, Type entityComponentType, Type fieldType = null)
        {
            if (fieldType != null)
            {
                throw new EntityComponentException(message, entityComponentType, fieldType);
            }

            throw new EntityComponentException(message, entityComponentType);
        }

        static readonly Type DISPATCHONCHANGETYPE       = typeof(DispatchOnChange<>);
        static readonly Type DISPATCHONSETTYPE          = typeof(DispatchOnSet<>);
        static readonly Type EGIDType                   = typeof(EGID);
        static readonly Type EXCLUSIVEGROUPSTRUCTTYPE   = typeof(ExclusiveGroupStruct);
        static readonly Type SERIALIZABLE_ENTITY_STRUCT = typeof(SerializableEntityComponent);
        static readonly Type STRINGTYPE                 = typeof(string);

        internal static readonly Type ENTITY_STRUCT_INFO_VIEW = typeof(EntityInfoViewComponent);
    }

    public class EntityComponentException : Exception
    {
        public EntityComponentException(string message, Type entityComponentType, Type type) :
            base(message.FastConcat(" entity view: '", entityComponentType.ToString(), "', field: '", type.ToString()))
        {
        }

        public EntityComponentException(string message, Type entityComponentType) :
            base(message.FastConcat(" entity view: ", entityComponentType.ToString()))
        {
        }
    }
}