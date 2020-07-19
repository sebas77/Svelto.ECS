#if DEBUG && !PROFILE_SVELTO
using System;
using System.Collections.Generic;
using Svelto.DataStructures;
#else
using System;
using System.Diagnostics;
#endif

namespace Svelto.ECS
{
    /// <summary>
    ///     Note: this check doesn't catch the case when an add and remove is done on the same entity before the nextI am
    ///     submission. Two operations on the same entity are not allowed between submissions.
    /// </summary>
    public partial class EnginesRoot
    {
#if DEBUG && !PROFILE_SVELTO
        void CheckRemoveEntityID(EGID egid, Type entityComponent, string caller = "")
        {
            if (_idCheckers.TryGetValue(egid.groupID, out var hash))
            {
                if (hash.Contains(egid.entityID) == false)
                    throw new ECSException("Entity with not found ID is about to be removed: id: "
                                          .FastConcat(" caller: ", caller, " ")
                                          .FastConcat(egid.entityID).FastConcat(" groupid: ").FastConcat(egid.groupID.ToName())
                                          .FastConcat(" type: ").FastConcat(entityComponent != null ? entityComponent.Name : "not available"));

                hash.Remove(egid.entityID);

                if (hash.Count == 0)
                    _idCheckers.Remove(egid.groupID);
            }
            else
            {
                throw new ECSException("Entity with not found ID is about to be removed: id: "
                                      .FastConcat(" caller: ", caller, " ")
                                      .FastConcat(egid.entityID).FastConcat(" groupid: ").FastConcat(egid.groupID.ToName())
                                      .FastConcat(" type: ").FastConcat(entityComponent != null ? entityComponent.Name : "not available"));
            }
        }

        void CheckAddEntityID(EGID egid, Type entityComponent, string caller = "")
        {
//            Console.LogError("<color=orange>added</color> ".FastConcat(egid.ToString()));

            if (_idCheckers.TryGetValue(egid.groupID, out var hash) == false)
                hash = _idCheckers[egid.groupID] = new HashSet<uint>();
            else
                if (hash.Contains(egid.entityID))
                    throw new ECSException("Entity with used ID is about to be built: '"
                                          .FastConcat("' id: '").FastConcat(egid.entityID).FastConcat("' groupid: '")
                                          .FastConcat(egid.groupID.ToName()).FastConcat(" ", entityComponent != null ? entityComponent.Name : "not available")
                                          .FastConcat("'"));

            hash.Add(egid.entityID);
        }

        void RemoveGroupID(ExclusiveGroupStruct groupID) { _idCheckers.Remove(groupID); }

        readonly FasterDictionary<uint, HashSet<uint>> _idCheckers = new FasterDictionary<uint, HashSet<uint>>();
#else
        [Conditional("_CHECKS_DISABLED")]
        void CheckRemoveEntityID(EGID egid, Type entityComponent, string caller = "")
        {
        }

        [Conditional("_CHECKS_DISABLED")]
        void CheckAddEntityID(EGID egid, Type entityComponen, string caller = "")
        {
        }
        
        [Conditional("_CHECKS_DISABLED")]
        void RemoveGroupID(ExclusiveGroupStruct groupID)
        {
        }
#endif
    }
}