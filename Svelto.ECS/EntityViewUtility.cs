using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.Utilities;

namespace Svelto.ECS
{

#if DEBUG && !PROFILER
    struct ECSTuple<T1, T2>
    {
        public readonly T1 implementorType;
        public          T2 numberOfImplementations;

        public ECSTuple(T1 implementor, T2 v)
        {
            implementorType         = implementor;
            numberOfImplementations = v;
        }
    }
#endif


    static class EntityViewUtility
    {

        public static void FillEntityView<T>(this IEntityBuilder entityBuilder
                                           , ref  T              entityView
                                           , FasterList<KeyValuePair<Type, ActionCast<T>>>
                                                 entityViewBlazingFastReflection
                                           , object[] implementors,
#if DEBUG && !PROFILER
                                             Dictionary<Type, ECSTuple<object, int>> implementorsByType
#else
                                            Dictionary<Type, object> implementorsByType
#endif
                                           , Dictionary<Type, Type[]> cachedTypes
        )
        {
            //efficient way to collect the fields of every EntityViewType
            var setters =
                FasterList<KeyValuePair<Type, ActionCast<T>>>
                   .NoVirt.ToArrayFast(entityViewBlazingFastReflection, out var count);

            for (var index = 0; index < implementors.Length; index++)
            {
                var implementor = implementors[index];

                if (implementor != null)
                {
                    var type = implementor.GetType();

                    if (cachedTypes.TryGetValue(type, out var interfaces) == false)
                        interfaces = cachedTypes[type] = type.GetInterfacesEx();

                    for (var iindex = 0; iindex < interfaces.Length; iindex++)
                    {
                        var componentType = interfaces[iindex];
#if DEBUG && !PROFILER
                        if (implementorsByType.TryGetValue(componentType, out var implementorData))
                        {
                            implementorData.numberOfImplementations++;
                            implementorsByType[componentType] = implementorData;
                        }
                        else
                            implementorsByType[componentType] = new ECSTuple<object, int>(implementor, 1);
#else
                        implementorsByType[componentType] = implementor;
#endif
                    }
                }
#if DEBUG && !PROFILER
                else
                {
                    Svelto.Console.Log(NULL_IMPLEMENTOR_ERROR.FastConcat(" entityView ",
                                                                         entityBuilder.GetEntityType().ToString()));
                }
#endif
            }

            for (var i = 0; i < count; i++)
            {
                var fieldSetter = setters[i];
                var fieldType   = fieldSetter.Key;

#if DEBUG && !PROFILER
                ECSTuple<object, int> component;
#else
            object component;
#endif

                if (implementorsByType.TryGetValue(fieldType, out component) == false)
                {
                    var e = new ECSException(NOT_FOUND_EXCEPTION + " Component Type: " + fieldType.Name +
                                             " - EntityView: "   + entityBuilder.GetEntityType().Name);

                    throw e;
                }
#if DEBUG && !PROFILER
                if (component.numberOfImplementations > 1)
                    throw new ECSException(DUPLICATE_IMPLEMENTOR_ERROR.FastConcat(
                                                                                  "Component Type: ", fieldType.Name,
                                                                                  " implementor: ",
                                                                                  component.implementorType
                                                                                           .ToString()) +
                                           " - EntityView: "                                            +
                                           entityBuilder.GetEntityType().Name);
#endif
#if DEBUG && !PROFILER
                fieldSetter.Value(ref entityView, component.implementorType);
#else
            fieldSetter.Value(ref entityView, component);
#endif
            }

            implementorsByType.Clear();
        }

        const string DUPLICATE_IMPLEMENTOR_ERROR =
            "<color=orange>Svelto.ECS</color> the same component is implemented with more than one implementor. This is " +
            "considered an error and MUST be fixed. ";

        const string NULL_IMPLEMENTOR_ERROR =
            "<color=orange>Svelto.ECS</color> Null implementor, please be careful about the implementors passed to avoid " +
            "performance loss ";

        const string NOT_FOUND_EXCEPTION = "<color=orange>Svelto.ECS</color> Implementor not found for an EntityView. ";
    }
}