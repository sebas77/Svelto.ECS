namespace Svelto.ECS
{
    /// <summary>
    /// Inherit from an ExtendibleEntityDescriptor to extend a base entity descriptor that can be used
    /// to swap and remove specialized entities from abstract engines
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public abstract class ExtendibleEntityDescriptor<TType>:IEntityDescriptor where TType : IEntityDescriptor, new()
    {
        protected ExtendibleEntityDescriptor(IEntityBuilder[] extraEntities)
        {
            _dynamicDescriptor = new DynamicEntityDescriptor<TType>(extraEntities);
        }

        public IEntityBuilder[] entitiesToBuild => _dynamicDescriptor.entitiesToBuild;

        readonly DynamicEntityDescriptor<TType> _dynamicDescriptor;
    }
}