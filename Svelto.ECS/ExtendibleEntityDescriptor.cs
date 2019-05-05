namespace Svelto.ECS
{
    public abstract class ExtendibleEntityDescriptor<TType>:IEntityDescriptor where TType : IEntityDescriptor, new()
    {
        protected ExtendibleEntityDescriptor(IEntityBuilder[] extraEntities)
        {
            _dynamicDescriptor = new DynamicEntityDescriptor<TType>(extraEntities);
        }

        public IEntityBuilder[] entitiesToBuild { get { return _dynamicDescriptor.entitiesToBuild; } }

        readonly DynamicEntityDescriptor<TType> _dynamicDescriptor;
    }
}