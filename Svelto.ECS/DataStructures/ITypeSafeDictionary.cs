using System;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeDictionary<TValue> : ITypeSafeDictionary where TValue : IEntityComponent
    {
        void Add(uint egidEntityId, in TValue entityComponent);
        ref TValue this[uint idEntityId] { get; }
        bool TryGetValue(uint entityId, out TValue item);
        ref TValue GetOrCreate(uint idEntityId);

        IBuffer<TValue> GetValues(out uint count);
        ref TValue GetDirectValueByRef(uint key);
    }

    public interface ITypeSafeDictionary:IDisposable
    {
        uint count { get; }
        ITypeSafeDictionary Create();

        void AddEntitiesToEngines(FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityComponentEnginesDb,
            ITypeSafeDictionary realDic, ExclusiveGroupStruct @group, in PlatformProfiler profiler);
        void RemoveEntitiesFromEngines(FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityComponentEnginesDB,
            in PlatformProfiler profiler, ExclusiveGroupStruct @group);
        void AddEntitiesFromDictionary(ITypeSafeDictionary entitiesToSubmit, uint groupId);
        void MoveEntityFromEngines(EGID fromEntityGid, EGID? toEntityID, ITypeSafeDictionary toGroup,
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines, in PlatformProfiler profiler);
        void AddEntityToDictionary(EGID fromEntityGid, EGID toEntityID, ITypeSafeDictionary toGroup);
        void RemoveEntityFromDictionary(EGID fromEntityGid);

        void SetCapacity(uint size);
        void Trim();
        void Clear();
        void FastClear();
        bool Has(uint key);
        bool ContainsKey(uint egidEntityId);
        uint GetIndex(uint valueEntityId);
        bool TryFindIndex(uint entityGidEntityId, out uint index);
    }
}