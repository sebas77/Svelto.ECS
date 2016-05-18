namespace Svelto.ES
{
    public interface IEnginesRoot
    {
        void AddEngine(IEngine engine);
    }

    public interface IEntityFactory
    {
        void BuildEntity(int ID, EntityDescriptor ED);
    }
}
