namespace Svelto.ECS
{
    public class ExtendibleEntityDescriptor<TType>:IEntityDescriptor where TType : IEntityDescriptor, new()
    {
        protected ExtendibleEntityDescriptor(IEntityBuilder[] extraEntities)
        {
            _dynamicDescriptor = new  DynamicEntityDescriptorInfo<TType>(extraEntities);
        }

        public IEntityBuilder[] entitiesToBuild { get { return _dynamicDescriptor.entitiesToBuild; } }

        readonly DynamicEntityDescriptorInfo<TType> _dynamicDescriptor;
    }
}