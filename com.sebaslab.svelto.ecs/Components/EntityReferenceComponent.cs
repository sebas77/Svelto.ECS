using Svelto.ECS.Reference;

namespace Svelto.ECS
{
    //To do: this should be removed and the only reason it exists is to solve some edge case scenarios with
    //the publish/consumer pattern
    public struct EntityReferenceComponent:IEntityComponent, INeedEntityReference
    {
        public EntityReference selfReference { get; set; }
    }
}