using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    ///IEntityComponents are unmanaged struct components stored in native memory. If they are not unmanaged they won't be recognised as IEntityComponent!
    public interface IEntityComponent:_IInternalEntityComponent
    {
    }
}