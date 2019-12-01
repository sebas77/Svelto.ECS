#if !DEBUG || PROFILER
#define DISABLE_CHECKS
using System.Diagnostics;
#endif
using System;
using System.Reflection;

namespace Svelto.ECS
{
    internal static class EntityBuilderUtilities
    {
        const string MSG = "Entity Structs field and Entity View Struct components must hold value types.";


#if DISABLE_CHECKS
        [Conditional("_CHECKS_DISABLED")]
#endif
        public static void CheckFields(Type entityStructType, bool needsReflection)
        {
            if (entityStructType == ENTITY_STRUCT_INFO_VIEW ||
                entityStructType == EGIDType ||
                entityStructType == EXCLUSIVEGROUPSTRUCTTYPE ||
                entityStructType == SERIALIZABLE_ENTITY_STRUCT)
            {
                return;
            }

            if (needsReflection == false)
            {
                if (entityStructType.IsClass)
                {
                    throw new EntityStructException("EntityStructs must be structs.", entityStructType);
                }

                FieldInfo[] fields = entityStructType.GetFields(BindingFlags.Public | BindingFlags.Instance);

                for (var i = fields.Length - 1; i >= 0; --i)
                {
                    FieldInfo fieldInfo = fields[i];
                    Type fieldType = fieldInfo.FieldType;

                    SubCheckFields(fieldType, entityStructType);
                }
            }
            else
            {
                FieldInfo[] fields = entityStructType.GetFields(BindingFlags.Public | BindingFlags.Instance);

                if (fields.Length < 1)
                {
                    ProcessError("Entity View Structs must hold only entity components interfaces.", entityStructType);
                }

                for (int i = fields.Length - 1; i >= 0; --i)
                {
                    FieldInfo fieldInfo = fields[i];

                    if (fieldInfo.FieldType.IsInterfaceEx() == false)
                    {
                        ProcessError("Entity View Structs must hold only entity components interfaces.",
                            entityStructType);
                    }

                    PropertyInfo[] properties = fieldInfo.FieldType.GetProperties(
                        BindingFlags.Public |
                        BindingFlags.Instance |
                        BindingFlags.DeclaredOnly);

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
                            SubCheckFields(propertyType, entityStructType);
                        }
                    }
                }
            }
        }

        static void SubCheckFields(Type fieldType, Type entityStructType)
        {
            if (fieldType.IsPrimitive || fieldType.IsValueType)
            {
                if (fieldType.IsValueType && !fieldType.IsEnum && fieldType.IsPrimitive == false)
                {
                    CheckFields(fieldType, false);
                }

                return;
            }

            ProcessError(MSG, entityStructType, fieldType);
        }

        static void ProcessError(string message, Type entityViewType, Type fieldType = null)
        {
            if (fieldType != null)
            {
                throw new EntityStructException(message, entityViewType, fieldType);
            }

            throw new EntityStructException(message, entityViewType);
        }

        static readonly Type DISPATCHONCHANGETYPE       = typeof(DispatchOnChange<>);
        static readonly Type DISPATCHONSETTYPE          = typeof(DispatchOnSet<>);
        static readonly Type EGIDType                   = typeof(EGID);
        static readonly Type EXCLUSIVEGROUPSTRUCTTYPE   = typeof(ExclusiveGroup.ExclusiveGroupStruct);
        static readonly Type SERIALIZABLE_ENTITY_STRUCT = typeof(SerializableEntityStruct);
        static readonly Type STRINGTYPE                 = typeof(string);

        internal static readonly Type ENTITY_STRUCT_INFO_VIEW = typeof(EntityStructInfoView);
    }

    public class EntityStructException : Exception
    {
        public EntityStructException(string message, Type entityViewType, Type type) :
            base(message.FastConcat(" entity view: '", entityViewType.ToString(), "', field: '", type.ToString()))
        {
        }

        public EntityStructException(string message, Type entityViewType) :
            base(message.FastConcat(" entity view: ", entityViewType.ToString()))
        {
        }
    }
}