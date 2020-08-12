namespace Svelto.ECS
{
    public interface IEntityLocatorMap
    {
        EntityLocator GetLocator(EGID egid);

        bool TryGetEGID(EntityLocator locator, out EGID egid);
    }
}