#if SLOW_SVELTO_SUBMISSION
namespace Svelto.ECS
{
    /// <summary>
    /// The use of this is an exception and it's necessary for deprecated design only
    /// It currently exist because of the publisher/consumer behavior, but the publisher/consumer must not be
    /// considered an ECS pattern.
    /// Other uses are invalid.
    /// It will become obsolete over the time
    /// </summary>
    public interface INeedEntityReference
    {
        EntityReference selfReference { get; set; }
    }
}
#endif