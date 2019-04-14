namespace Svelto.ECS
{
    ///<summary>EntityStruct MUST implement IEntiyStruct</summary>
    public interface IEntityStruct
    {}
    
    public interface INeedEGID
    {
        EGID ID { get; set; }
    }
}