#if SLOW_SVELTO_SUBMISSION
namespace Svelto.ECS
{
    public struct EGIDComponent:IEntityComponent, INeedEGID
    {
        public EGID ID { get; set; }
    }
}
#endif