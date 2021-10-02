using Svelto.ECS.Reference;

namespace Svelto.ECS
{
    /// <summary>
    /// The use of this is an exception and it's necessary for deprecated design only
    /// It currently exist because of the publisher/consumer behavior, but the publisher/consumer must not be
    /// considered an ECS pattern.
    /// Other uses are invalid.
    /// </summary>
    public interface INeedEntityReference
    {
        EntityReference selfReference { get; set; }
    }
}