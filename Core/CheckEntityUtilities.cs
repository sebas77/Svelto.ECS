//#define VERBOSE
#if !DEBUG || PROFILE_SVELTO
#define DONT_USE
using System.Diagnostics;
#endif
using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    /// <summary>
    ///     Note: this check doesn't catch the case when an add and remove is done on the same entity before the next
    ///     submission. Two operations on the same entity are not allowed between submissions.
    /// </summary>
    public partial class EnginesRoot
    {
        enum OperationType
        {
            Add,
            Remove,
            SwapFrom,
            SwapTo
        }
        
#if DONT_USE
        [Conditional("MEANINGLESS")]
#endif
        void InitDebugChecks()
        {
            _multipleOperationOnSameEGIDChecker = new FasterDictionary<EGID, OperationType>();
            _idChecker = new FasterDictionary<ExclusiveGroupStruct, HashSet<uint>>();
        }
        
#if DONT_USE
        [Conditional("MEANINGLESS")]
#endif
        void CheckSwapEntityID(EGID fromEgid, EGID toEgid, Type entityDescriptorType, string caller)
        {
            if (_multipleOperationOnSameEGIDChecker.TryGetValue(fromEgid, out var fromOperationType) == true)
            {
                var operationName = OperationName(fromOperationType);
                throw new ECSException(
                    "Executing multiple structural changes (swapFrom) on the same entity is not supported "
                           .FastConcat(" caller: ", caller, " ").FastConcat(fromEgid.entityID).FastConcat(" groupid: ")
                           .FastConcat(fromEgid.groupID.ToName()).FastConcat(" type: ")
                           .FastConcat(entityDescriptorType != null ? entityDescriptorType.Name : "not available")
                           .FastConcat(" previous operation was: ")
                           .FastConcat(operationName));
            }

            if (_multipleOperationOnSameEGIDChecker.TryGetValue(toEgid, out var toOperationType) == true)
            {
                var operationName = OperationName(toOperationType);
                throw new ECSException(
                    "Executing multiple structural changes (swapTo) on the same entity is not supported "
                           .FastConcat(" caller: ", caller, " ").FastConcat(toEgid.entityID).FastConcat(" groupid: ")
                           .FastConcat(toEgid.groupID.ToName()).FastConcat(" type: ")
                           .FastConcat(entityDescriptorType != null ? entityDescriptorType.Name : "not available")
                           .FastConcat(" previous operation was: ")
                           .FastConcat(operationName));
            }

            HashRemove(fromEgid, entityDescriptorType, false, caller);
            HashAdd(toEgid, entityDescriptorType, caller);

            _multipleOperationOnSameEGIDChecker.Add(fromEgid, OperationType.SwapFrom);
            _multipleOperationOnSameEGIDChecker.Add(toEgid, OperationType.SwapTo);
        }

#if DONT_USE
        [Conditional("MEANINGLESS")]
#endif
        void CheckRemoveEntityID(EGID egid, Type entityDescriptorType, string caller)
        {
            bool isAllowed = false;
            if (_multipleOperationOnSameEGIDChecker.TryGetValue(egid, out var operationType) == true)
            {
                //remove supersedes swap and remove operations, this means remove is allowed if the previous operation was swap or remove on the same submission
                isAllowed = operationType == OperationType.Remove || operationType == OperationType.SwapFrom;

                if (isAllowed)
                {
#if VERBOSE                    
                    var operationName = OperationName(operationType);
                    Console.LogDebugWarning(
                        "Executing multiple structural changes (remove) in one submission on the same entity. Remove supersedes swap and remove operations "
                               .FastConcat(" caller: ", caller, " ").FastConcat(egid.entityID).FastConcat(" groupid: ")
                               .FastConcat(egid.groupID.ToName()).FastConcat(" type: ")
                               .FastConcat(entityDescriptorType != null ? entityDescriptorType.Name : "not available")
                               .FastConcat(" previous operation was: ")
                               .FastConcat(operationName));
#endif
                }
                else
                    throw new ECSException(
                        "Executing multiple structural changes (remove) in one submission on the same entity is not supported "
                               .FastConcat(" caller: ", caller, " ").FastConcat(egid.entityID).FastConcat(" groupid: ")
                               .FastConcat(egid.groupID.ToName()).FastConcat(" type: ")
                               .FastConcat(entityDescriptorType != null ? entityDescriptorType.Name : "not available")
                               .FastConcat(" previous operation was: ")
                               .FastConcat("add"));
            }

            HashRemove(egid, entityDescriptorType, isAllowed, caller);

            if (isAllowed == false)
                _multipleOperationOnSameEGIDChecker.Add(egid, OperationType.Remove);
            else
                _multipleOperationOnSameEGIDChecker[egid] = OperationType.Remove;
        }

#if DONT_USE
        [Conditional("MEANINGLESS")]
#endif
        void CheckAddEntityID(EGID egid, Type entityDescriptorType, string caller)
        {
            if (_multipleOperationOnSameEGIDChecker.TryGetValue(egid, out var operationType) == true)
            {
                var operationName = OperationName(operationType);

                throw new ECSException(
                    "Executing multiple structural changes (build) on the same entity is not supported "
                           .FastConcat(" caller: ", caller, " ").FastConcat(egid.entityID).FastConcat(" groupid: ")
                           .FastConcat(egid.groupID.ToName()).FastConcat(" type: ")
                           .FastConcat(entityDescriptorType != null ? entityDescriptorType.Name : "not available")
                           .FastConcat(" previous operation was: ")
                           .FastConcat(operationName));
            }

            HashAdd(egid, entityDescriptorType, caller);

            _multipleOperationOnSameEGIDChecker.Add(egid, OperationType.Add);
        }

#if DONT_USE
        [Conditional("MEANINGLESS")]
#endif
        void RemoveGroupID(ExclusiveBuildGroup groupID)
        {
            _idChecker.Remove(groupID);
        }

#if DONT_USE
        [Conditional("MEANINGLESS")]
#endif
        void ClearChecksForMultipleOperationsOnTheSameEgid()
        {
            _multipleOperationOnSameEGIDChecker.Clear();
        }

        void HashRemove(EGID egid, Type entityDescriptorType, bool isAllowed, string caller)
        {
            if (_idChecker.TryGetValue(egid.groupID, out HashSet<uint> hash))
            {
                if (hash.Contains(egid.entityID) == false && isAllowed == false)
                    throw new ECSException(
                        "Trying to remove an Entity not present in the database "
                               .FastConcat(" caller: ", caller, " entityID ").FastConcat(egid.entityID).FastConcat(" groupid: ")
                               .FastConcat(egid.groupID.ToName()).FastConcat(" type: ")
                               .FastConcat(entityDescriptorType != null ? entityDescriptorType.Name : "not available"));
            }
            else
            {
                throw new ECSException(
                    "Trying to remove an Entity with a group never used so far "
                           .FastConcat(" caller: ", caller, " entityID ").FastConcat(egid.entityID).FastConcat(" groupid: ")
                           .FastConcat(egid.groupID.ToName()).FastConcat(" type: ")
                           .FastConcat(entityDescriptorType != null ? entityDescriptorType.Name : "not available"));
            }

            hash.Remove(egid.entityID);
        }

        void HashAdd(EGID egid, Type entityDescriptorType, string caller)
        {
            var hash = _idChecker.GetOrAdd(egid.groupID, () => new HashSet<uint>());
            if (hash.Contains(egid.entityID) == true)
                throw new ECSException(
                    "Trying to add an Entity already present in the database "
                           .FastConcat(" caller: ", caller, " entityID ").FastConcat(egid.entityID)
                           .FastConcat(" groupid: ").FastConcat(egid.groupID.ToName()).FastConcat(" type: ")
                           .FastConcat(
                                entityDescriptorType != null
                                        ? entityDescriptorType.Name
                                        : "not available"));
            hash.Add(egid.entityID);
        }

        string OperationName(OperationType operationType)
        {
            string operationName;
            switch (operationType)
            {
                case OperationType.Remove:
                    operationName = "remove";
                    break;
                case OperationType.Add:
                    operationName = "add";
                    break;
                case OperationType.SwapFrom:
                    operationName = "swapFrom";
                    break;
                default:
                    operationName = "swapTo";
                    break;
            }

            return operationName;
        }

        DataStructures.FasterDictionary<EGID, OperationType> _multipleOperationOnSameEGIDChecker;
        DataStructures.FasterDictionary<ExclusiveGroupStruct, HashSet<uint>> _idChecker;
    }
}