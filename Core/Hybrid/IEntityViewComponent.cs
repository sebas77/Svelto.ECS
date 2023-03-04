using Svelto.ECS.Internal;

namespace Svelto.ECS.Hybrid
{
    ///IManagedComponents are pure struct components stored in managed memory
    public interface IManagedComponent:_IInternalEntityComponent
    {}
    
    /// IEntityViewComponents are components that leverage on the implementers pattern (not recommended in most cases)
    public interface IEntityViewComponent:IManagedComponent
#if SLOW_SVELTO_SUBMISSION
        ,INeedEGID
#endif    
    {}
}

