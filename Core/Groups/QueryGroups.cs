using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Experimental
{
    struct GroupsList
    {
        public static GroupsList Init()
        {
            var group = new GroupsList();

            group._groups = new FasterList<ExclusiveGroupStruct>();
            group._sets   = new HashSet<ExclusiveGroupStruct>();

            return group;
        }

        public void Reset()
        {
            _sets.Clear();
        }

        public void AddRange(ExclusiveGroupStruct[] groupsToAdd, int length)
        {
            for (var i = 0; i < length; i++) _sets.Add(groupsToAdd[i]);
        }

        public void Add(ExclusiveGroupStruct group)
        {
            _sets.Add(group);
        }

        public void Exclude(ExclusiveGroupStruct[] groupsToIgnore, int length)
        {
            for (var i = 0; i < length; i++) _sets.Remove(groupsToIgnore[i]);
        }

        public void Exclude(ExclusiveGroupStruct groupsToIgnore)
        {
            _sets.Remove(groupsToIgnore);
        }

        public void Resize(uint preparecount)
        {
            _groups.Resize(preparecount);
        }

        public FasterList<ExclusiveGroupStruct> Evaluate()
        {
            _groups.Clear();

            foreach (var item in _sets) _groups.Add(item);

            return _groups;
        }
        
        FasterList<ExclusiveGroupStruct> _groups;
        HashSet<ExclusiveGroupStruct>    _sets;
    }

    public ref struct QueryGroups
    {
        static readonly ThreadLocal<GroupsList> groups;

        static QueryGroups()
        {
            groups = new ThreadLocal<GroupsList>(GroupsList.Init);
        }

        public QueryGroups(LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            var groupsValue = QueryGroups.groups.Value;

            groupsValue.Reset();
            groupsValue.AddRange(groups.ToArrayFast(out var count), count);
        }

        public QueryGroups(ExclusiveGroupStruct group)
        {
            var groupsValue = groups.Value;

            groupsValue.Reset();
            groupsValue.Add(group);
        }

        public QueryGroups(uint preparecount)
        {
            var groupsValue = groups.Value;

            groupsValue.Reset();
            groupsValue.Resize(preparecount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QueryGroups Union(ExclusiveGroupStruct group)
        {
            var groupsValue = groups.Value;

            groupsValue.Add(group);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QueryGroups Union(LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            var groupsValue = QueryGroups.groups.Value;

            groupsValue.AddRange(groups.ToArrayFast(out var count), count);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QueryGroups Except(ExclusiveGroupStruct group)
        {
            var groupsValue = groups.Value;

            groupsValue.Exclude(group);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QueryGroups Except(ExclusiveGroupStruct[] groupsToIgnore)
        {
            var groupsValue = groups.Value;

            groupsValue.Exclude(groupsToIgnore, groupsToIgnore.Length);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QueryGroups Except(LocalFasterReadOnlyList<ExclusiveGroupStruct> groupsToIgnore)
        {
            var groupsValue = groups.Value;

            groupsValue.Exclude(groupsToIgnore.ToArrayFast(out var count), count);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QueryGroups Except(FasterList<ExclusiveGroupStruct> groupsToIgnore)
        {
            var groupsValue = groups.Value;

            groupsValue.Exclude(groupsToIgnore.ToArrayFast(out var count), count);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QueryGroups Except(FasterReadOnlyList<ExclusiveGroupStruct> groupsToIgnore)
        {
            var groupsValue = groups.Value;

            groupsValue.Exclude(groupsToIgnore.ToArrayFast(out var count), count);

            return this;
        }

        // public QueryGroups WithAny<T>(EntitiesDB entitiesDB)
        //     where T : struct, IEntityComponent
        // {
        //     var group       = groups.Value.reference;
        //     var groupsCount = group.count;
        //     
        //     for (uint i = 0; i < groupsCount; i++)
        //     {
        //         if (entitiesDB.Count<T>(group[i]) == 0)
        //         {
        //             group.UnorderedRemoveAt(i);
        //             i--;
        //             groupsCount--;
        //         }
        //     }
        //
        //     return this;
        // }

        public QueryResult Evaluate()
        {
            var groupsValue = groups.Value;

            return new QueryResult(groupsValue.Evaluate());
        }
        
        public void Evaluate(FasterList<ExclusiveGroupStruct> group)
        {
            var groupsValue = groups.Value;

            groupsValue.Evaluate().CopyTo(group.ToArrayFast(out var count), count);
        }
    }

    public readonly ref struct QueryResult
    {
        public QueryResult(FasterList<ExclusiveGroupStruct> group)
        {
            _group = group;
        }

        public LocalFasterReadOnlyList<ExclusiveGroupStruct> result => _group;

        readonly FasterReadOnlyList<ExclusiveGroupStruct> _group;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T>(EntitiesDB entitiesDB) where T : struct, _IInternalEntityComponent
        {
            var count = 0;

            var groupsCount                             = result.count;
            for (var i = 0; i < groupsCount; ++i) count += entitiesDB.Count<T>(result[i]);

            return count;
        }

        public int Max<T>(EntitiesDB entitiesDB) where T : struct, _IInternalEntityComponent
        {
            var max = 0;

            var groupsCount                           = result.count;
            for (var i = 0; i < groupsCount; ++i)
            {
                var count = entitiesDB.Count<T>(result[i]);
                if (count > max) max = count;
            }

            return max;
        }
    }
}