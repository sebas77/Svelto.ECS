namespace Svelto.ECS
{
    ///<summary>Entity Components MUST implement IEntityComponent</summary>
    public interface IEntityComponent
    {
    }

    /// <summary>
    /// use INeedEGID on an IEntityComponent only if you need the EGID. consider using EGIDComponent instead
    /// </summary>
    public interface INeedEGID
    {
        //The set is used only for the framework, but it must stay there
        EGID ID { get; set; }
    }
}