using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    /// <summary>
    /// Inherit from an ExtendibleEntityDescriptor to extend a base entity descriptor that can be used
    /// to swap and remove specialized entities from abstract engines
    ///
    /// Usage Example:
    ///
    /// class SpecialisedDescriptor : ExtendibleEntityDescriptor<BaseDescriptor>
    /// {
    /// public SpecialisedDescriptor() : base (new IComponentBuilder[]
    /// {
    ///     new ComponentBuilder<ObjectParentComponent>() //add more components to the base descriptor
    /// })
    /// {
    ///    ExtendWith<ContractDescriptor>(); //add extra components from descriptors that act as contract
    /// }
    /// }
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public abstract class ExtendibleEntityDescriptor<TType> : IDynamicEntityDescriptor where TType : IEntityDescriptor, new()
    {
        static ExtendibleEntityDescriptor()
        {
            //I am removing this check because in reality there is not a strong reason to forbid it and
            //furthermore it's already possible to extend a SerializableEntityDescriptor through DynamicEntityDescriptor
            // if (typeof(ISerializableEntityDescriptor).IsAssignableFrom(typeof(TType)))
            //     throw new Exception(
            //         $"SerializableEntityDescriptors cannot be used as base entity descriptor: {typeof(TType)}");
        }

        protected ExtendibleEntityDescriptor(IComponentBuilder[] extraEntities)
        {
            _dynamicDescriptor = new DynamicEntityDescriptor<TType>(extraEntities);
        }

        protected ExtendibleEntityDescriptor()
        {
            _dynamicDescriptor = DynamicEntityDescriptor<TType>.CreateDynamicEntityDescriptor();
        }

        protected ExtendibleEntityDescriptor<TType> ExtendWith<T>() where T : IEntityDescriptor, new()
        {
            _dynamicDescriptor.ExtendWith<T>();

            return this;
        }

        protected ExtendibleEntityDescriptor<TType> ExtendWith(IComponentBuilder[] extraEntities)
        {
            _dynamicDescriptor.ExtendWith(extraEntities);

            return this;
        }

        protected void Add<T>() where T : struct, _IInternalEntityComponent
        {
            _dynamicDescriptor.Add<T>();
        }
        protected void Add<T, U>() where T : struct,  _IInternalEntityComponent where U : struct,  _IInternalEntityComponent
        {
            _dynamicDescriptor.Add<T, U>();
        }
        protected void Add<T, U, V>() where T : struct,  _IInternalEntityComponent where U : struct,  _IInternalEntityComponent where V : struct,  _IInternalEntityComponent
        {
            _dynamicDescriptor.Add<T, U, V>();
        }

        public IComponentBuilder[] componentsToBuild => _dynamicDescriptor.componentsToBuild;

        DynamicEntityDescriptor<TType> _dynamicDescriptor;
    }
}