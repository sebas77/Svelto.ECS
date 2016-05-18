namespace Svelto.ES
{
	/// <summary>
	/// please use [DisallowMultipleComponent] in your monobehaviours that implement IEntityDescriptorHolder
	/// </summary>
    public interface IEntityDescriptorHolder
    {
        EntityDescriptor BuildDescriptorType();
    }
}
