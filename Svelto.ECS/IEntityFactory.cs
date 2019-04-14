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
        ///  <summary>
        /// where performance is critical, you may wish to pre allocate the space needed
        /// to store the entities
        ///  </summary>
        ///  <typeparam name="T"></typeparam>
        ///  <param name="groupStructId"></param>
        ///  <param name="size"></param>
        void PreallocateEntitySpace<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId, uint size)
            where T : IEntityDescriptor, new();
        
        /// <summary>
        ///     The EntityDescriptor doesn't need to be ever instantiated. It just describes the Entity
        ///     itself in terms of EntityViews to build. The Implementors are passed to fill the
        ///     references of the EntityViews components. Please read the articles on my blog
        ///     to understand better the terminologies
        ///     Using this function is like building a normal entity, but the entity views
        ///     are grouped by groupID to be more efficiently processed inside engines and
        ///     improve cache locality. Either class entityViews and struct entityViews can be
        ///     grouped.
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="groupStructId"></param>
        /// <param name="ed"></param>
        /// <param name="implementors"></param>
        EntityStructInitializer BuildEntity<T>(uint entityID, ExclusiveGroup.ExclusiveGroupStruct groupStructId,
                                               object[] implementors = null) 
            where T : IEntityDescriptor, new();
        EntityStructInitializer BuildEntity<T>(EGID egid, object[] implementors = null) 
            where T:IEntityDescriptor, new();

#if REAL_ID        
        /// <summary>
        /// BuildEntity version without specifying the entity ID. The complete EGID will be found inside
        /// the EntityStructInitializer and/or the single entity components
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="implementors"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        EntityStructInitializer BuildEntity<T>(ExclusiveGroup.ExclusiveGroupStruct groupID, object[] implementors = null)
            where T : IEntityDescriptor, new();
#endif
        
        /// <summary>
        ///     When the type of the entity is not known (this is a special case!) an EntityDescriptorInfo
        ///     can be built in place of the generic parameter T.
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="entityDescriptor"></param>
        /// <param name="implementors"></param>
        /// 
        EntityStructInitializer BuildEntity<T>(uint      entityID, ExclusiveGroup.ExclusiveGroupStruct groupStructId,
                                               T        descriptorEntity,
                                               object[] implementors = null)
            where T : IEntityDescriptor;
        EntityStructInitializer BuildEntity<T>(EGID egid, T entityDescriptor, object[] implementors = null)
            where T : IEntityDescriptor;
    }
}
