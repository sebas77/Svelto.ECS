namespace Svelto.ECS
{
    /// <summary>
    /// please use [DisallowMultipleComponent] in your monobehaviours that implement IEntityDescriptorHolder
    /// </summary>
    public interface IEntityDescriptorHolder
    {
        //I must find a nicer solution for the extraImplentors
        EntityDescriptor BuildDescriptorType(object[] extraImplentors = null);
    }
}
