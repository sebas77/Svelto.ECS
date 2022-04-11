namespace Svelto.ECS
{
    public static class EntityDescriptorExtension
    {
        public static bool IsUnmanaged(this IEntityDescriptor descriptor)
        {
            foreach (IComponentBuilder component in descriptor.componentsToBuild)
                if (component.GetEntityComponentType() != typeof(EntityInfoComponent) && component.isUnmanaged == false)
                    return false;
            
            return true;
        }
    }
}