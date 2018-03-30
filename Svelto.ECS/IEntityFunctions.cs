namespace Svelto.ECS
{
    public interface IEntityFunctions
    {
        //being entity ID globally not unique, the group must be specified when
        //an entity is removed. Not specificing the group will attempt to remove
        //the entity from the special standard group.
        void RemoveEntity(int entityID);
        void RemoveEntity(int entityID, int groupID);
        void RemoveEntity(EGID entityegid);

        void RemoveGroupAndEntities(int groupID);
        
        void SwapEntityGroup(int entityID, int fromGroupID, int toGroupID);
        void SwapEntityGroup(int entityID, int toGroupID);
    }
}