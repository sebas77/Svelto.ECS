namespace Svelto.ECS.Hybrid
{
    public interface IManagedComponent:IEntityComponent
    {}
    
    public interface IEntityViewComponent:IManagedComponent
#if SLOW_SVELTO_SUBMISSION
        ,INeedEGID
#endif    
    {}
}

