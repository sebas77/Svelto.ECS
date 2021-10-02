namespace Svelto.ECS
{
    /// <summary>
    /// use INeedEGID on an IEntityComponent only if you need the EGID. consider using EGIDComponent instead
    /// INeedEGID and EGIDComponent will become probably obsolete once QueryEntities will be able to return
    /// also the EGIDs to iterate upon
    /// </summary>
    public interface INeedEGID
    {
        //The set is used only by the framework, but it must stay there
        EGID ID { get; set; }
    }
}