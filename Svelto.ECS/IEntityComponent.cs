namespace Svelto.ECS
{
    ///<summary>EntityComponent MUST implement IEntityComponent</summary>
    public interface IEntityComponent
    {
    }

    /// <summary>
    /// use INeedEGID on an IEntityComponent only if you need the EGID
    /// </summary>
    public interface INeedEGID
    {
        //The set is used only for the framework, but it must stay there
        EGID ID { get; set; }
    }
}