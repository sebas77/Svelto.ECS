#if !DEBUG || PROFILER
#define DISABLE_CHECKS
using System.Diagnostics;
#endif
using System;
using System.Collections.Generic;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
#if DISABLE_CHECKS        
        [Conditional("_CHECKS_DISABLED")]
#endif        
        void CheckRemoveEntityID(EGID entityID, IEntityDescriptor descriptorEntity)
        {

            Dictionary<Type, ITypeSafeDictionary> group;
            var                                   descriptorEntitiesToBuild = descriptorEntity.entitiesToBuild;
            
            if (_groupEntityDB.TryGetValue(entityID.groupID, out group))
            {
                for (int i = 0; i < descriptorEntitiesToBuild.Length; i++)
                {
                    CheckRemoveEntityID(entityID, descriptorEntitiesToBuild[i].GetEntityType(), group, descriptorEntity.ToString());
                }
            }
            else
            {
                Console.LogError("Entity with not found ID is about to be removed: id: "
                                                           .FastConcat(entityID.entityID)
                                                           .FastConcat(" groupid: ")
                                                           .FastConcat(entityID.groupID));
            }
        }

#if DISABLE_CHECKS        
        [Conditional("_CHECKS_DISABLED")]
#endif        
        void CheckRemoveEntityID(EGID entityID, Type entityType, Dictionary<Type, ITypeSafeDictionary> group, string name)
        {
            ITypeSafeDictionary entities;
            if (group.TryGetValue(entityType, out entities))
            {
                if (entities.Has(entityID.entityID) == false)
                {
                    Console.LogError("Entity ".FastConcat(name, " with not found ID is about to be removed: ")
                                                               .FastConcat(entityType.ToString())
                                                               .FastConcat(" id: ")
                                                               .FastConcat(entityID.entityID)
                                                               .FastConcat(" groupid: ")
                                                               .FastConcat(entityID.groupID));
                }
            }
            else
            {
                Console.LogError("Entity ".FastConcat(name, " with not found ID is about to be removed: ")
                                                           .FastConcat(entityType.ToString())
                                                           .FastConcat(" id: ")
                                                           .FastConcat(entityID.entityID)
                                                           .FastConcat(" groupid: ")
                                                           .FastConcat(entityID.groupID));
            }
        }

#if DISABLE_CHECKS        
        [Conditional("_CHECKS_DISABLED")]
#endif        
        void CheckAddEntityID<T>(EGID entityID, T descriptorEntity) where T:IEntityDescriptor
        {
            Dictionary<Type, ITypeSafeDictionary> group;
            var                                   descriptorEntitiesToBuild = descriptorEntity.entitiesToBuild;
            
            //these are the entities added in this frame
            if (_groupEntityDB.TryGetValue(entityID.groupID, out group))
            {
                for (int i = 0; i < descriptorEntitiesToBuild.Length; i++)
                {
                    CheckAddEntityID(entityID, descriptorEntitiesToBuild[i].GetEntityType(), group, descriptorEntity.ToString());
                }
            }
        }

#if DISABLE_CHECKS        
        [Conditional("_CHECKS_DISABLED")]
#endif        
        static void CheckAddEntityID(EGID entityID, Type entityType, Dictionary<Type, ITypeSafeDictionary> group, string name)
        {
            ITypeSafeDictionary entities;
            if (group.TryGetValue(entityType, out entities))
            {
                if (entities.Has(entityID.entityID))
                {
                    Console.LogError("Entity ".FastConcat(name, " with used ID is about to be built: ")
                                                               .FastConcat(entityType.ToString())
                                                               .FastConcat(" id: ")
                                                               .FastConcat(entityID.entityID)
                                                               .FastConcat(" groupid: ")
                                                               .FastConcat(entityID.groupID));
                }
            }
        }
    }
}