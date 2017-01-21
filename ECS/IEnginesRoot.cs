namespace Svelto.ECS
{
    public interface IEnginesRoot
    {
        void AddEngine(IEngine engine);
    }

    public interface IEntityFactory
    {
        void BuildEntity(int ID, EntityDescriptor ED);

        void BuildEntityGroup(int ID, EntityDescriptor ED);
    }
}
