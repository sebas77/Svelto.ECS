namespace Svelto.ECS
{   
    public interface IEntityFactory
    {
        void Preallocate<T>(int size) where T : IEntityDescriptor, new();

        void BuildEntity<T>(int entityID, object[] implementors = null) where T:IEntityDescriptor, new();
        void BuildEntity(int entityID, EntityDescriptorInfo entityDescriptor, object[] implementors = null);

        void BuildMetaEntity<T>(int metaEntityID, object[] implementors = null) where T:IEntityDescriptor, new();

        void BuildEntityInGroup<T>(int entityID, int groupID, object[] implementors = null) where T:IEntityDescriptor, new();
        void BuildEntityInGroup(int entityID, int groupID, EntityDescriptorInfo entityDescriptor, object[] implementors = null);
    }
    
    public interface IEntityFunctions
    {
        void RemoveEntity(int entityID, IRemoveEntityComponent removeInfo);

        void RemoveEntity<T>(int entityID) where T:IEntityDescriptor, new();
        
        void RemoveMetaEntity<T>(int metaEntityID) where T:IEntityDescriptor, new();

        void RemoveEntityFromGroup<T>(int entityID, int groupID) where T:IEntityDescriptor, new();
    }
}
