namespace Svelto.ECS
{
    /// <summary>
    /// Entities are always built in group. Where the group is not specificed, a special standard group is used
    /// ID can be reused within groups
    /// an EnginesRoot reference cannot be held by anything else than the Composition Root
    /// where it has been created. IEntityFactory and IEntityFunctions allow a weakreference
    /// of the EnginesRoot to be passed around.
    ///
    /// ExclusiveGroups must be used in your game like:
    /// static class GameExclusiveGroup
    ///{
    ///   public static readonly ExclusiveGroups PlayerEntitiesGroup = new ExclusiveGroups();
    ///}
    /// 
    /// </summary>
    public interface IEntityFactory
    {
    /// <summary>
        ///where performance is critical, you may wish to pre allocate the space needed
        ///to store the entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="size"></param>
        void PreallocateEntitySpace<T>(int size) where T : IEntityDescriptor, new();
        void PreallocateEntitySpace<T>(ExclusiveGroup groupID, int size) where T : IEntityDescriptor, new();
        
        /// <summary>
        ///     The EntityDescriptor doesn't need to be ever instantiated. It just describes the Entity
        ///     itself in terms of EntityViews to build. The Implementors are passed to fill the
        ///     references of the EntityViews components. Please read the articles on my blog
        ///     to understand better the terminologies
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityID"></param>
        /// <param name="implementors"></param>
        EntityStructInitializer BuildEntity<T>(int entityID, object[] implementors) where T:IEntityDescriptor, new();


        /// <summary>
        ///     Using this function is like building a normal entity, but the entityViews
        ///     are grouped by groupID to be more efficently processed inside engines and
        ///     improve cache locality. Either class entityViews and struct entityViews can be
        ///     grouped.
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="groupID"></param>
        /// <param name="ed"></param>
        /// <param name="implementors"></param>
        EntityStructInitializer BuildEntity<T>(int entityID, ExclusiveGroup groupID, object[] implementors) where T:IEntityDescriptor, new();
        EntityStructInitializer BuildEntity<T>(EGID egid, object[] implementors) where T:IEntityDescriptor, new();
        

        /// <summary>
        ///     When the type of the entity is not known (this is a special case!) an EntityDescriptorInfo
        ///     can be built in place of the generic parameter T.
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="entityDescriptor"></param>
        /// <param name="implementors"></param>
        /// 
        EntityStructInitializer BuildEntity(int entityID, ExclusiveGroup groupID, IEntityDescriptor descriptorEntity, object[] implementors);
        EntityStructInitializer BuildEntity(int entityID, IEntityDescriptor descriptorEntity, object[] implementors);
        EntityStructInitializer BuildEntity(EGID egid, IEntityDescriptor descriptorEntity, object[] implementors);
    }
}
