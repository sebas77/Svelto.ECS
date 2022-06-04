using System.Runtime.CompilerServices;

namespace Svelto.ECS
{
    public partial class EntitiesDB
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //Todo I should rename this method to reflect its original intention
        public void PublishEntityChange<T>(EGID egid) where T : unmanaged, IEntityComponent
        {
            //Note: it is correct to publish the EGID at the moment of the publishing, as the responsibility of 
            //the publisher consumer is not tracking the real state of the entity in the database at the 
            //moment of the consumption, but it's instead to store a copy of the entity at the moment of the publishing
            _entityStream.PublishEntity(ref this.QueryEntity<T>(egid), egid);
        }
    }
}