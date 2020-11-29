namespace Svelto.ECS
{
    public interface IEntityReferenceLocatorMap
    {
        EntityReference GetEntityReference(EGID egid);

        bool TryGetEGID(EntityReference reference, out EGID egid);
    }
}