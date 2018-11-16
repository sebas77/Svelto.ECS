using System;
using System.Collections.Generic;
using System.Diagnostics;
using Svelto.ECS.Internal;

#if DEBUG && !PROFILER
#define _USE_IT
#endif

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        [Conditional("_USE_IT")]
        void CheckRemoveEntityID(EGID entityID, IEntityDescriptor descriptorEntity)
        {

            Dictionary<Type, ITypeSafeDictionary> @group;
            var                                   descriptorEntitiesToBuild = descriptorEntity.entitiesToBuild;
            
            if (_groupEntityDB.TryGetValue(entityID.groupID, out @group) == true)
            {
                for (int i = 0; i < descriptorEntitiesToBuild.Length; i++)
                {
                    CheckRemoveEntityID(entityID, descriptorEntitiesToBuild[i].GetEntityType(), @group, descriptorEntity.ToString());
                }
            }
            else
            {
                Svelto.Utilities.Console.LogError("Entity ".FastConcat(" with not found ID is about to be removed: ")
                                                           .FastConcat(" id: ")
                                                           .FastConcat(entityID.entityID)
                                                           .FastConcat(" groupid: ")
                                                           .FastConcat(entityID.groupID));
            }
        }

        [Conditional("_USE_IT")]
        void CheckRemoveEntityID(EGID entityID, Type entityType, Dictionary<Type, ITypeSafeDictionary> @group, string name)
        {
            ITypeSafeDictionary entities;
            if (@group.TryGetValue(entityType, out entities) == true)
            {
                if (entities.Has(entityID.entityID) == false)
                {
                    Svelto.Utilities.Console.LogError("Entity ".FastConcat(name, " with not found ID is about to be removed: ")
                                                               .FastConcat(entityType)
                                                               .FastConcat(" id: ")
                                                               .FastConcat(entityID.entityID)
                                                               .FastConcat(" groupid: ")
                                                               .FastConcat(entityID.groupID));
                }
            }
            else
            {
                Svelto.Utilities.Console.LogError("Entity ".FastConcat(name, " with not found ID is about to be removed: ")
                                                           .FastConcat(entityType)
                                                           .FastConcat(" id: ")
                                                           .FastConcat(entityID.entityID)
                                                           .FastConcat(" groupid: ")
                                                           .FastConcat(entityID.groupID));
            }
        }

        [Conditional("_USE_IT")]
        void CheckAddEntityID<T>(EGID entityID, T descriptorEntity) where T:IEntityDescriptor
        {
            Dictionary<Type, ITypeSafeDictionary> @group;
            var                                   descriptorEntitiesToBuild = descriptorEntity.entitiesToBuild;
            
            //these are the entities added in this frame
            if (_groupEntityDB.TryGetValue(entityID.groupID, out @group) == true)
            {
                for (int i = 0; i < descriptorEntitiesToBuild.Length; i++)
                {
                    CheckAddEntityID(entityID, descriptorEntitiesToBuild[i].GetEntityType(), @group, descriptorEntity.ToString());
                }
            }
        }

        [Conditional("_USE_IT")]
        static void CheckAddEntityID(EGID entityID, Type entityType, Dictionary<Type, ITypeSafeDictionary> @group, string name)
        {
            ITypeSafeDictionary entities;
            if (@group.TryGetValue(entityType, out entities))
            {
                if (entities.Has(entityID.entityID) == true)
                {
                    Svelto.Utilities.Console.LogError("Entity ".FastConcat(name, " with used ID is about to be built: ")
                                                               .FastConcat(entityType)
                                                               .FastConcat(" id: ")
                                                               .FastConcat(entityID.entityID)
                                                               .FastConcat(" groupid: ")
                                                               .FastConcat(entityID.groupID));
                }
            }
        }
    }
}