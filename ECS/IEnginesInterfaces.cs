namespace Svelto.ECS
{   
    public interface IEntityFactory
    {
        void BuildEntity<T>(int entityID, object[] implementors = null) where T:IEntityDescriptor, new();
        void BuildEntity(int entityID, EntityDescriptorInfo entityDescriptor, object[] implementors = null);

        void BuildMetaEntity<T>(int metaEntityID, object[] implementors = null) where T:IEntityDescriptor, new();

        void BuildEntityInGroup<T>(int entityID, int groupID, object[] implementors = null) where T:IEntityDescriptor, new();
        void BuildEntityInGroup(int entityID, int groupID, EntityDescriptorInfo entityDescriptor, object[] implementors = null);
    }
    
    public interface IEntityFunctions
    {
        void RemoveEntity(int entityID, IRemoveEntityComponent template);

        void RemoveEntity<T>(int entityID) where T:IEntityDescriptor, new();
        
        void RemoveMetaEntity<T>(int metaEntityID) where T:IEntityDescriptor, new();

        void RemoveEntityFromGroup<T>(int entityID, int groupID) where T:IEntityDescriptor, new();
#if EXPERIMENTAL        
        void SetEntityActiveState<T>(int entityID, bool state) where T:IEntityDescriptor, new();
        
        void SetMetaEntityActiveState<T>(int metaEntityID, bool state) where T:IEntityDescriptor, new();
        
        void SetEntityInGroupActiveState<T>(int entityID, int group,  bool state) where T:IEntityDescriptor, new();
#endif    
    }
}
