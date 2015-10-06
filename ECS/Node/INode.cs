namespace Svelto.ES
{
    public interface INode
    {
    }

    public interface INodeWithReferenceID<out T> : INode where T : class
    {
        T ID { get; }
    }

    public interface INodeWithValueID<out T> : INode where T : struct
    {
        T ID { get; }
    }
}
