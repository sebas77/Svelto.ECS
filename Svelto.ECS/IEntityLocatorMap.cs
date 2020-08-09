namespace Svelto.ECS
{
    public interface IEntityLocatorMap
    {
        EntityLocator GetLocator(EGID egid);

        EGID GetEGID(EntityLocator locator);
    }
}