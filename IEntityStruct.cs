namespace Svelto.ECS
{
    ///<summary>EntityStruct MUST implement IEntityStruct</summary>
    public interface IEntityStruct
    {
    }

    /// <summary>
    /// use INeedEGID on an IEntityStruct only if you need the EGID
    /// </summary>
    public interface INeedEGID
    {
        EGID ID { get; set; }
    }
}