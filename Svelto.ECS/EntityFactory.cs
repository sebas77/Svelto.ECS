using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.Utilities;
using Console = Utility.Console;

namespace Svelto.ECS.Internal
{
    static class EntityFactory
    {
        internal static void BuildGroupedEntityViews(int entityID, int groupID,
                                                     Dictionary<int, Dictionary<Type, ITypeSafeList>> groupEntityViewsByType,
                                                     Dictionary<Type, ITypeSafeList> entityViewsByType,
                                                     IEntityDescriptorInfo eentityViewsToBuildDescriptor,
                                                     object[]              implementors)
        {
            var entityViewsToBuildDescriptor =
                eentityViewsToBuildDescriptor as EntityDescriptorInfo;
            Dictionary<Type, ITypeSafeList> groupedEntityViewsTyped;

            if (groupEntityViewsByType.TryGetValue(groupID, out groupedEntityViewsTyped) == false)
            {
                groupedEntityViewsTyped = new Dictionary<Type, ITypeSafeList>();
                groupEntityViewsByType.Add(groupID, groupedEntityViewsTyped);
            }

            InternalBuildEntityViews(entityID, groupedEntityViewsTyped, entityViewsToBuildDescriptor, implementors);

            var removeEntityView = EntityView<EntityInfoView>.BuildEntityView(entityID);

            removeEntityView.groupID     = groupID;
            removeEntityView.isInAGroup  = true;
            removeEntityView.entityViews = entityViewsToBuildDescriptor.entityViewsToBuild;

            AddEntityInfoView(entityViewsByType, removeEntityView);
        }

        internal static void BuildEntityViews(int                             entityID,
                                              Dictionary<Type, ITypeSafeList> entityViewsByType,
                                              IEntityDescriptorInfo           eentityViewsToBuildDescriptor,
                                              object[]                        implementors)
        {
            var entityViewsToBuildDescriptor = eentityViewsToBuildDescriptor as EntityDescriptorInfo;

            InternalBuildEntityViews(entityID, entityViewsByType, entityViewsToBuildDescriptor, implementors);

            var removeEntityView = EntityView<EntityInfoView>.BuildEntityView(entityID);
            removeEntityView.entityViews = entityViewsToBuildDescriptor.entityViewsToBuild;

            AddEntityInfoView(entityViewsByType, removeEntityView);
        }

        static void AddEntityInfoView(Dictionary<Type, ITypeSafeList> entityViewsByType,
                                      EntityInfoView                  removeEntityView)
        {
            ITypeSafeList list;

            if (entityViewsByType.TryGetValue(typeof(EntityInfoView), out list) == false)
                list = entityViewsByType[typeof(EntityInfoView)] =
                           new TypeSafeFasterListForECSForClasses<EntityInfoView>();

            (list as TypeSafeFasterListForECSForClasses<EntityInfoView>).Add(removeEntityView);
        }

        static void InternalBuildEntityViews(int                             entityID,
                                             Dictionary<Type, ITypeSafeList> entityViewsByType,
                                             IEntityDescriptorInfo           eentityViewsToBuildDescriptor,
                                             object[]                        implementors)
        {
            var entityViewsToBuildDescriptor = eentityViewsToBuildDescriptor as EntityDescriptorInfo;
            var entityViewsToBuild           = entityViewsToBuildDescriptor.entityViewsToBuild;
            var count                        = entityViewsToBuild.Length;

            for (var index = 0; index < count; index++)
            {
                var entityViewBuilder = entityViewsToBuild[index];
                var entityViewType    = entityViewBuilder.GetEntityViewType();

                var entityViewObjectToFill =
                    BuildEntityView(entityID, entityViewsByType, entityViewType, entityViewBuilder);

                if (entityViewBuilder.mustBeFilled)
                    FillEntityView(entityViewObjectToFill as EntityView
                                 , implementors
                                 , entityViewsToBuildDescriptor.name);
            }
        }

        static IEntityView BuildEntityView(int  entityID,       Dictionary<Type, ITypeSafeList> entityViewsByType,
                                           Type entityViewType, IEntityViewBuilder              entityViewBuilder)
        {
            ITypeSafeList entityViewsList;

            var entityViewsPoolWillBeCreated =
                entityViewsByType.TryGetValue(entityViewType, out entityViewsList) == false;

            IEntityView entityViewObjectToFill;

            //passing the undefined entityViewsByType inside the entityViewBuilder will allow
            //it to be created with the correct type and casted back to the undefined list.
            //that's how the list will be eventually of the target type.
            entityViewBuilder.BuildEntityViewAndAddToList(ref entityViewsList, entityID, out entityViewObjectToFill);

            if (entityViewsPoolWillBeCreated)
                entityViewsByType.Add(entityViewType, entityViewsList);

            return entityViewObjectToFill;
        }

        //this is used to avoid newing a dictionary every time, but it's used locally only and it's clearead for each use
#if DEBUG && !PROFILER
        static readonly Dictionary<Type, Tuple<object, int>> implementorsByType =
            new Dictionary<Type, Tuple<object, int>>();
#else
        static readonly Dictionary<Type, object> implementorsByType = new Dictionary<Type, object>();
#endif

        static void FillEntityView(EntityView entityView
                                 , object[]   implementors
                                 , string     entityDescriptorName)
        {
            int count;

            //Very efficent way to collect the fields of every EntityViewType
            var setters =
                FasterList<KeyValuePair<Type, CastedAction<EntityView>>>
                   .NoVirt.ToArrayFast(entityView.entityViewBlazingFastReflection, out count);

            if (count == 0) return;

            for (var index = 0; index < implementors.Length; index++)
            {
                var implementor = implementors[index];

                if (implementor != null)
                {
                    var type = implementor.GetType();

                    Type[] interfaces;
                    if (_cachedTypes.TryGetValue(type, out interfaces) == false)
                        interfaces = _cachedTypes[type] = type.GetInterfacesEx();

                    for (var iindex = 0; iindex < interfaces.Length; iindex++)
                    {
                        var componentType = interfaces[iindex];
#if DEBUG && !PROFILER
                        Tuple<object, int> implementorHolder;

                        if (implementorsByType.TryGetValue(componentType, out implementorHolder))
                            implementorHolder.numberOfImplementations++;
                        else
                            implementorsByType[componentType] = new Tuple<object, int>(implementor, 1);
#else
                        implementorsByType[componentType] = implementor;
#endif
                    }
                }
#if DEBUG && !PROFILER
                else
                {
                    Console.LogError(NULL_IMPLEMENTOR_ERROR.FastConcat(entityView.ToString()));
                }
#endif
            }

            for (var i = 0; i < count; i++)
            {
                var fieldSetter = setters[i];
                var fieldType   = fieldSetter.Key;

#if DEBUG && !PROFILER
                Tuple<object, int> component;
#else
                    object component;
#endif

                if (implementorsByType.TryGetValue(fieldType, out component) == false)
                {
                    var e = new Exception(NOT_FOUND_EXCEPTION + " Component Type: " + fieldType.Name +
                                          " - EntityView: " +
                                          entityView.GetType().Name + " - EntityDescriptor " + entityDescriptorName);

                    throw e;
                }
#if DEBUG && !PROFILER
                if (component.numberOfImplementations > 1)
                    Console.LogError(DUPLICATE_IMPLEMENTOR_ERROR.FastConcat(
                                                                            "Component Type: ", fieldType.Name,
                                                                            " implementor: ",
                                                                            component.implementorType.ToString()) +
                                     " - EntityView: " +
                                     entityView.GetType().Name + " - EntityDescriptor " + entityDescriptorName);
#endif
#if DEBUG && !PROFILER
                fieldSetter.Value.Call(entityView, component.implementorType);
#else
                fieldSetter.Value.Call(entityView, component);
#endif
            }

            implementorsByType.Clear();
        }
#if DEBUG && !PROFILER
        struct Tuple<T1, T2>
        {
            public readonly T1 implementorType;
            public          T2 numberOfImplementations;

            public Tuple(T1 implementor, T2 v)
            {
                implementorType         = implementor;
                numberOfImplementations = v;
            }
        }
#endif
        static readonly Dictionary<Type, Type[]> _cachedTypes = new Dictionary<Type, Type[]>();

        const string DUPLICATE_IMPLEMENTOR_ERROR =
            "<color=orange>Svelto.ECS</color> the same component is implemented with more than one implementor. This is considered an error and MUST be fixed. ";

        const string NULL_IMPLEMENTOR_ERROR =
            "<color=orange>Svelto.ECS</color> Null implementor, please be careful about the implementors passed to avoid performance loss ";

        const string NOT_FOUND_EXCEPTION = "<color=orange>Svelto.ECS</color> Implementor not found for an EntityView. ";
    }
}