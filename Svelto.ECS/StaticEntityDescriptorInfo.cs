namespace Svelto.ECS
{
    class StaticEntityDescriptorInfo<TType>: IEntityDescriptor where TType : IEntityDescriptor
    {
        internal StaticEntityDescriptorInfo(TType descriptor)
        {
            entitiesToBuild = descriptor.entitiesToBuild;
        }

        public IEntityBuilder[] entitiesToBuild { get; }
    }
}

