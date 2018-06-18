namespace Svelto.ECS
{
    ///<summary>EntityStruct MUST implement IEntiyStruct</summary>
    public interface IEntityStruct
    {
        EGID ID { get; set; }
    }
}