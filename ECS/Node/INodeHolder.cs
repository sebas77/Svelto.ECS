namespace Svelto.ES
{
    interface INodeHolder
    {
        INode               node       { get; }
        INodeEnginesRoot    engineRoot { set; }
    }
}
