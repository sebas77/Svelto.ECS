namespace Svelto.ECS
{
    public interface IEnginesRoot
    {
        void AddEngine(IEngine engine);
    }

    public interface IEntityFactory
    {
        void BuildEntity(int ID, EntityDescriptor ED);

        void BuildMetaEntity(int metaEntityID, EntityDescriptor ED);

        void BuildEntityInGroup(int entityID, int groupID, EntityDescriptor ED);
    }
}
