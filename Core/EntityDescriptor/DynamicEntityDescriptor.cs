using System;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    /// <summary>
    /// DynamicEntityDescriptor can be used to add entity components to an existing EntityDescriptor that act as flags,
    /// at building time.
    /// This method allocates, so it shouldn't be abused
    /// TODO:Unit test cases where there could be duplicates of components, especially EntityInfoComponent.
    /// Test DynamicED of DynamicED
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public struct DynamicEntityDescriptor<TType> : IDynamicEntityDescriptor where TType : IEntityDescriptor, new()
    {
        public static DynamicEntityDescriptor<TType> CreateDynamicEntityDescriptor()
        {
            var entityDescriptor = new DynamicEntityDescriptor<TType>();
            
            var defaultEntities = EntityDescriptorTemplate<TType>.realDescriptor.componentsToBuild;
            var length          = defaultEntities.Length;

            if (FetchEntityInfoComponent(defaultEntities) == -1)
            {
                entityDescriptor._componentsToBuild = new IComponentBuilder[length + 1];

                Array.Copy(defaultEntities, 0, entityDescriptor._componentsToBuild, 0, length);
                //assign it after otherwise the previous copy will overwrite the value in case the item
                //is already present
                entityDescriptor._componentsToBuild[length] = new ComponentBuilder<EntityInfoComponent>(new EntityInfoComponent
                {
                    componentsToBuild = entityDescriptor._componentsToBuild
                });
            }
            else
            {
                entityDescriptor._componentsToBuild = new IComponentBuilder[length];

                Array.Copy(defaultEntities, 0, entityDescriptor._componentsToBuild, 0, length);
            }

            return entityDescriptor;
        }

        public DynamicEntityDescriptor(IComponentBuilder[] extraEntityBuilders)
        {
            this = DynamicEntityDescriptor<TType>.CreateDynamicEntityDescriptor();
            
            var extraEntitiesLength = extraEntityBuilders.Length;

            _componentsToBuild = Construct(extraEntitiesLength, extraEntityBuilders);
        }

        public DynamicEntityDescriptor(FasterList<IComponentBuilder> extraEntityBuilders) 
        {
            this = DynamicEntityDescriptor<TType>.CreateDynamicEntityDescriptor();
            
            var extraEntities       = extraEntityBuilders.ToArrayFast(out _);
            var extraEntitiesLength = extraEntityBuilders.count;

            _componentsToBuild = Construct((int)extraEntitiesLength, extraEntities);
        }

        public void ExtendWith<T>() where T : IEntityDescriptor, new()
        {
            var extraEntities = EntityDescriptorTemplate<T>.realDescriptor.componentsToBuild;

            _componentsToBuild = Construct(extraEntities.Length, extraEntities);
        }

        public void ExtendWith(IComponentBuilder[] extraEntities)
        {
            _componentsToBuild = Construct(extraEntities.Length, extraEntities);
        }

        public void ExtendWith(FasterList<IComponentBuilder> extraEntities)
        {
            _componentsToBuild = Construct(extraEntities.count, extraEntities.ToArrayFast(out _));
        }

        public void Add<T>() where T : struct, _IInternalEntityComponent
        {
            IComponentBuilder[] extraEntities = { new ComponentBuilder<T>() };
            _componentsToBuild = Construct(extraEntities.Length, extraEntities);
        }

        public void Add<T, U>() where T : struct, _IInternalEntityComponent where U : struct, _IInternalEntityComponent
        {
            IComponentBuilder[] extraEntities = { new ComponentBuilder<T>(), new ComponentBuilder<U>() };
            _componentsToBuild = Construct(extraEntities.Length, extraEntities);
        }

        public void Add<T, U, V>() where T : struct, _IInternalEntityComponent
                                   where U : struct, _IInternalEntityComponent
                                   where V : struct, _IInternalEntityComponent
        {
            IComponentBuilder[] extraEntities =
            {
                new ComponentBuilder<T>(), new ComponentBuilder<U>(), new ComponentBuilder<V>()
            };
            _componentsToBuild = Construct(extraEntities.Length, extraEntities);
        }

        /// <summary>
        /// Note: unluckily I didn't design the serialization system to be component order independent, so unless
        /// I do something about it, this method cannot be optimized, the logic of the component order must stay
        /// untouched (no reordering, no use of dictionaries). Components order must stay as it comes, as
        /// well as extracomponents order.
        /// Speed, however, is not a big issue for this class, as the data is always composed once per entity descriptor
        /// at static constructor time
        /// </summary>
        /// <returns></returns>
        IComponentBuilder[] Construct(int extraComponentsLength, IComponentBuilder[] extraComponents)
        {
            IComponentBuilder[] MergeLists
                (IComponentBuilder[] startingComponents, IComponentBuilder[] newComponents, int newComponentsLength)
            {
                var startComponents =
                    new FasterDictionary<RefWrapper<IComponentBuilder, ComponentBuilderComparer>, IComponentBuilder>();
                var xtraComponents =
                    new FasterDictionary<RefWrapper<IComponentBuilder, ComponentBuilderComparer>, IComponentBuilder>();

                for (uint i = 0; i < startingComponents.Length; i++)
                    startComponents
                            [new RefWrapper<IComponentBuilder, ComponentBuilderComparer>(startingComponents[i])] =
                        startingComponents[i];

                for (uint i = 0; i < newComponentsLength; i++)
                    xtraComponents[new RefWrapper<IComponentBuilder, ComponentBuilderComparer>(newComponents[i])] =
                        newComponents[i];

                xtraComponents.Exclude(startComponents);

                if (newComponentsLength != xtraComponents.count)
                {
                    newComponentsLength = xtraComponents.count;

                    uint index = 0;
                    foreach (var couple in xtraComponents)
                        newComponents[index++] = couple.key.type;
                }

                IComponentBuilder[] componentBuilders =
                    new IComponentBuilder[newComponentsLength + startingComponents.Length];

                Array.Copy(startingComponents, 0, componentBuilders, 0, startingComponents.Length);
                Array.Copy(newComponents, 0, componentBuilders, startingComponents.Length, newComponentsLength);

                var entityInfoComponentIndex = FetchEntityInfoComponent(componentBuilders);
                
                DBC.ECS.Check.Assert(entityInfoComponentIndex != -1);

                componentBuilders[entityInfoComponentIndex] = new ComponentBuilder<EntityInfoComponent>(
                    new EntityInfoComponent
                    {
                        componentsToBuild = componentBuilders
                    });

                return componentBuilders;
            }

            if (extraComponentsLength == 0)
            {
                return _componentsToBuild;
            }

            var safeCopyOfExtraComponents = new IComponentBuilder[extraComponentsLength];
            Array.Copy(extraComponents, safeCopyOfExtraComponents, extraComponentsLength);

            return MergeLists(_componentsToBuild, safeCopyOfExtraComponents, extraComponentsLength);
        }

        static int FetchEntityInfoComponent(IComponentBuilder[] defaultEntities)
        {
            int length = defaultEntities.Length;
            int index  = -1;

            for (var i = 0; i < length; i++)
            {
                //the special entity already exists
                if (defaultEntities[i].GetEntityComponentType() == ComponentBuilderUtilities.ENTITY_INFO_COMPONENT)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        public IComponentBuilder[] componentsToBuild => _componentsToBuild;

        IComponentBuilder[] _componentsToBuild;
    }
}