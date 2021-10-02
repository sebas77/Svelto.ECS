using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;

namespace Svelto.ECS.Experimental
{
    struct GroupsList
    {
        static GroupsList()
        {
            groups = new FasterList<ExclusiveGroupStruct>();
            sets   = new HashSet<ExclusiveGroupStruct>();
        }

        static readonly FasterList<ExclusiveGroupStruct> groups;
        static readonly HashSet<ExclusiveGroupStruct>    sets;

        public void Reset() { sets.Clear(); }

        public void AddRange(ExclusiveGroupStruct[] groupsToAdd, int length)
        {
            for (int i = 0; i < length; i++)
            {
                sets.Add(groupsToAdd[i]);
            }
        }

        public void Add(ExclusiveGroupStruct @group) { sets.Add(group); }

        public void Exclude(ExclusiveGroupStruct[] groupsToIgnore, int length)
        {
            for (int i = 0; i < length; i++)
            {
                sets.Remove(groupsToIgnore[i]);
            }
        }

        public void Exclude(ExclusiveGroupStruct groupsToIgnore) { sets.Remove(groupsToIgnore); }

        public void EnsureCapacity(uint preparecount) { groups.EnsureCapacity(preparecount); }

        public FasterList<ExclusiveGroupStruct> Evaluate()
        {
            groups.FastClear();

            foreach (var item in sets)
            {
                groups.Add(item);
            }

            return groups;
        }
    }

    public ref struct QueryGroups
    {
        static readonly ThreadLocal<GroupsList> groups = new ThreadLocal<GroupsList>();

        public QueryGroups(LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            var groupsValue = QueryGroups.groups.Value;

            groupsValue.Reset();
             groupsValue.AddRange(groups.ToArrayFast(out var count), count);
        }

        public QueryGroups(ExclusiveGroupStruct @group)
        {
            var groupsValue = groups.Value;

            groupsValue.Reset();
            groupsValue.Add(@group);
        }

        public QueryGroups(uint preparecount)
        {
            var groupsValue = groups.Value;

            groupsValue.Reset();
            groupsValue.EnsureCapacity(preparecount);
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
            var groupsValue = QueryGroups.groups.Value;

            groupsValue.Exclude(groupsToIgnore, groupsToIgnore.Length);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QueryGroups Except(LocalFasterReadOnlyList<ExclusiveGroupStruct> groupsToIgnore)
        {
            var groupsValue = QueryGroups.groups.Value;

            groupsValue.Exclude(groupsToIgnore.ToArrayFast(out var count), count);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QueryGroups Except(FasterList<ExclusiveGroupStruct> groupsToIgnore)
        {
            var groupsValue = QueryGroups.groups.Value;

            groupsValue.Exclude(groupsToIgnore.ToArrayFast(out var count), count);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QueryGroups Except(FasterReadOnlyList<ExclusiveGroupStruct> groupsToIgnore)
        {
            var groupsValue = QueryGroups.groups.Value;

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
    }

    public readonly ref struct QueryResult
    {
        public QueryResult(FasterList<ExclusiveGroupStruct> @group) { _group = @group; }

        public LocalFasterReadOnlyList<ExclusiveGroupStruct> result => _group;

        readonly FasterReadOnlyList<ExclusiveGroupStruct> _group;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T>(EntitiesDB entitiesDB)
            where T : struct, IEntityComponent
        {
            int count = 0;

            var groupsCount = result.count;
            for (int i = 0; i < groupsCount; ++i)
            {
                count += entitiesDB.Count<T>(result[i]);
            }

            return count;
        }
    }
}