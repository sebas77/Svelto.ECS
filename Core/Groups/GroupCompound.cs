using System;
using System.Collections.Generic;
using System.Threading;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public abstract class GroupCompound<G1, G2, G3, G4> where G1 : GroupTag<G1>
                                                        where G2 : GroupTag<G2>
                                                        where G3 : GroupTag<G3>
                                                        where G4 : GroupTag<G4>
    {
        static readonly FasterList<ExclusiveGroupStruct> _Groups;
        static readonly HashSet<ExclusiveGroupStruct>    _GroupsHashSet;

        public static FasterReadOnlyList<ExclusiveGroupStruct> Groups =>
            new FasterReadOnlyList<ExclusiveGroupStruct>(_Groups);

        public static ExclusiveBuildGroup BuildGroup => new ExclusiveBuildGroup(_Groups[0]);

        static int isInitializing;

        static GroupCompound()
        {
            if (Interlocked.CompareExchange(ref isInitializing, 1, 0) == 0)
            {
                _Groups = new FasterList<ExclusiveGroupStruct>(1);

                var Group = new ExclusiveGroup();
                _Groups.Add(Group);
                _GroupsHashSet = new HashSet<ExclusiveGroupStruct>(_Groups.ToArrayFast(out _));

                GroupCompound<G1, G2, G3>.Add(Group);
                GroupCompound<G1, G2, G4>.Add(Group);
                GroupCompound<G1, G3, G4>.Add(Group);
                GroupCompound<G2, G3, G4>.Add(Group);

                GroupCompound<G1, G2>.Add(Group); //<G1/G2> and <G2/G1> must share the same array
                GroupCompound<G1, G3>.Add(Group);
                GroupCompound<G1, G4>.Add(Group);
                GroupCompound<G2, G3>.Add(Group);
                GroupCompound<G2, G4>.Add(Group);
                GroupCompound<G3, G4>.Add(Group);

                //This is done here to be sure that the group is added once per group tag
                //(if done inside the previous group compound it would be added multiple times)
                GroupTag<G1>.Add(Group);
                GroupTag<G2>.Add(Group);
                GroupTag<G3>.Add(Group);
                GroupTag<G4>.Add(Group);

#if DEBUG
                GroupNamesMap.idToName[(uint) Group] =
                    $"Compound: {typeof(G1).Name}-{typeof(G2).Name}-{typeof(G3).Name}-{typeof(G4).Name} ID {(uint) Group}";
#endif
                 GroupHashMap.RegisterGroup(BuildGroup,
                    $"Compound: {typeof(G1).Name}-{typeof(G2).Name}-{typeof(G3).Name}-{typeof(G4).Name}");

                //all the combinations must share the same group and group hashset
                GroupCompound<G1, G2, G4, G3>._Groups = _Groups;
                GroupCompound<G1, G3, G2, G4>._Groups = _Groups;
                GroupCompound<G1, G3, G4, G2>._Groups = _Groups;
                GroupCompound<G1, G4, G2, G3>._Groups = _Groups;
                GroupCompound<G2, G1, G3, G4>._Groups = _Groups;
                GroupCompound<G2, G3, G4, G1>._Groups = _Groups;
                GroupCompound<G3, G1, G2, G4>._Groups = _Groups;
                GroupCompound<G4, G1, G2, G3>._Groups = _Groups;
                GroupCompound<G1, G4, G3, G2>._Groups = _Groups;
                GroupCompound<G2, G1, G4, G3>._Groups = _Groups;
                GroupCompound<G2, G4, G3, G1>._Groups = _Groups;
                GroupCompound<G3, G1, G4, G2>._Groups = _Groups;
                GroupCompound<G4, G1, G3, G2>._Groups = _Groups;
                GroupCompound<G2, G3, G1, G4>._Groups = _Groups;
                GroupCompound<G3, G4, G1, G2>._Groups = _Groups;
                GroupCompound<G2, G4, G1, G3>._Groups = _Groups;
                GroupCompound<G3, G2, G1, G4>._Groups = _Groups;
                GroupCompound<G3, G2, G4, G1>._Groups = _Groups;
                GroupCompound<G3, G4, G2, G1>._Groups = _Groups;
                GroupCompound<G4, G2, G1, G3>._Groups = _Groups;
                GroupCompound<G4, G2, G3, G1>._Groups = _Groups;
                GroupCompound<G4, G3, G1, G2>._Groups = _Groups;
                GroupCompound<G4, G3, G2, G1>._Groups = _Groups;

                GroupCompound<G1, G2, G4, G3>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G1, G3, G2, G4>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G1, G3, G4, G2>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G1, G4, G2, G3>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G2, G1, G3, G4>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G2, G3, G4, G1>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G3, G1, G2, G4>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G4, G1, G2, G3>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G1, G4, G3, G2>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G2, G1, G4, G3>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G2, G4, G3, G1>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G3, G1, G4, G2>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G4, G1, G3, G2>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G2, G3, G1, G4>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G3, G4, G1, G2>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G2, G4, G1, G3>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G3, G2, G1, G4>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G3, G2, G4, G1>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G3, G4, G2, G1>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G4, G2, G1, G3>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G4, G2, G3, G1>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G4, G3, G1, G2>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G4, G3, G2, G1>._GroupsHashSet = _GroupsHashSet;
            }
        }

        internal static void Add(ExclusiveGroupStruct @group)
        {
            for (int i = 0; i < _Groups.count; ++i)
                if (_Groups[i] == group)
                    throw new Exception("temporary must be transformed in unit test");

            _Groups.Add(group);
            _GroupsHashSet.Add(group);
        }

        public static bool Includes(ExclusiveGroupStruct @group) { return _GroupsHashSet.Contains(@group); }
    }

    public abstract class GroupCompound<G1, G2, G3>
        where G1 : GroupTag<G1> where G2 : GroupTag<G2> where G3 : GroupTag<G3>
    {
        static readonly FasterList<ExclusiveGroupStruct> _Groups;
        static readonly HashSet<ExclusiveGroupStruct>    _GroupsHashSet;

        public static FasterReadOnlyList<ExclusiveGroupStruct> Groups =>
            new FasterReadOnlyList<ExclusiveGroupStruct>(_Groups);

        public static ExclusiveBuildGroup BuildGroup => new ExclusiveBuildGroup(_Groups[0]);
        
        static int isInitializing;

        internal static void Add(ExclusiveGroupStruct group)
        {
            for (var i = 0; i < _Groups.count; ++i)
                if (_Groups[i] == group)
                    throw new Exception("temporary must be transformed in unit test");

            _Groups.Add(group);
            _GroupsHashSet.Add(group);
        }

        public static bool Includes(ExclusiveGroupStruct @group) { return _GroupsHashSet.Contains(@group); }

        static GroupCompound()
        {
            if (Interlocked.CompareExchange(ref isInitializing, 1, 0) == 0)
            {
                _Groups = new FasterList<ExclusiveGroupStruct>(1);

                var Group = new ExclusiveGroup();
                _Groups.Add(Group);
                _GroupsHashSet = new HashSet<ExclusiveGroupStruct>(_Groups.ToArrayFast(out _));

                GroupCompound<G1, G2>.Add(Group); //<G1/G2> and <G2/G1> must share the same array
                GroupCompound<G1, G3>.Add(Group);
                GroupCompound<G2, G3>.Add(Group);

                //This is done here to be sure that the group is added once per group tag
                //(if done inside the previous group compound it would be added multiple times)
                GroupTag<G1>.Add(Group);
                GroupTag<G2>.Add(Group);
                GroupTag<G3>.Add(Group);

#if DEBUG
                GroupNamesMap.idToName[(uint) Group] =
                    $"Compound: {typeof(G1).Name}-{typeof(G2).Name}-{typeof(G3).Name} ID {(uint) Group}";
#endif
                GroupHashMap.RegisterGroup(BuildGroup,
                    $"Compound: {typeof(G1).Name}-{typeof(G2).Name}-{typeof(G3).Name}");
                
                //all the combinations must share the same group and group hashset
                GroupCompound<G3, G1, G2>._Groups = _Groups;
                GroupCompound<G2, G3, G1>._Groups = _Groups;
                GroupCompound<G3, G2, G1>._Groups = _Groups;
                GroupCompound<G1, G3, G2>._Groups = _Groups;
                GroupCompound<G2, G1, G3>._Groups = _Groups;

                GroupCompound<G3, G1, G2>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G2, G3, G1>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G3, G2, G1>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G1, G3, G2>._GroupsHashSet = _GroupsHashSet;
                GroupCompound<G2, G1, G3>._GroupsHashSet = _GroupsHashSet;
            }
        }
    }

    public abstract class GroupCompound<G1, G2> where G1 : GroupTag<G1> where G2 : GroupTag<G2>
    {
        static readonly FasterList<ExclusiveGroupStruct> _Groups;
        static readonly HashSet<ExclusiveGroupStruct>    _GroupsHashSet;

        public static FasterReadOnlyList<ExclusiveGroupStruct> Groups =>
            new FasterReadOnlyList<ExclusiveGroupStruct>(_Groups);

        public static ExclusiveBuildGroup BuildGroup => new ExclusiveBuildGroup(_Groups[0]);
        
        static int isInitializing;

        internal static void Add(ExclusiveGroupStruct group)
        {
            for (var i = 0; i < _Groups.count; ++i)
                if (_Groups[i] == group)
                    throw new Exception("temporary must be transformed in unit test");

            _Groups.Add(group);
            _GroupsHashSet.Add(group);
        }

        public static bool Includes(ExclusiveGroupStruct @group) { return _GroupsHashSet.Contains(@group); }

        static GroupCompound()
        {
            if (Interlocked.CompareExchange(ref isInitializing, 1, 0) == 0)
            {
                var Group = new ExclusiveGroup();

                _Groups = new FasterList<ExclusiveGroupStruct>(1);
                _Groups.Add(Group);
                _GroupsHashSet = new HashSet<ExclusiveGroupStruct>(_Groups.ToArrayFast(out _));

                //every abstract group preemptively adds this group, it may or may not be empty in future
                GroupTag<G1>.Add(Group);
                GroupTag<G2>.Add(Group);

#if DEBUG
                GroupNamesMap.idToName[(uint) Group] = $"Compound: {typeof(G1).Name}-{typeof(G2).Name} ID {(uint) Group}";
#endif
                 GroupHashMap.RegisterGroup(BuildGroup,
                    $"Compound: {typeof(G1).Name}-{typeof(G2).Name}");

                GroupCompound<G2, G1>._Groups                  = _Groups;
                GroupCompound<G2, G1>._GroupsHashSet           = _GroupsHashSet;
            }
        }
    }

    /// <summary>
    ///A Group Tag holds initially just a group, itself. However the number of groups can grow with the number of
    ///combinations of GroupTags including this one. This because a GroupTag is an adjective and different entities
    ///can use the same adjective together with other ones. However since I need to be able to iterate over all the
    ///groups with the same adjective, a group tag needs to hold all the groups sharing it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GroupTag<T> where T : GroupTag<T>
    {
        static readonly FasterList<ExclusiveGroupStruct> _Groups = new FasterList<ExclusiveGroupStruct>(1);
        static readonly HashSet<ExclusiveGroupStruct>    _GroupsHashSet;

        public static FasterReadOnlyList<ExclusiveGroupStruct> Groups =>
            new FasterReadOnlyList<ExclusiveGroupStruct>(_Groups);

        public static ExclusiveBuildGroup BuildGroup => new ExclusiveBuildGroup(_Groups[0]);
        
        static int isInitializing;

        static GroupTag()
        {
            if (Interlocked.CompareExchange(ref isInitializing, 1, 0) == 0)
            {
                var group = new ExclusiveGroup();
                _Groups.Add(group);
                _GroupsHashSet = new HashSet<ExclusiveGroupStruct>(_Groups.ToArrayFast(out _));

#if DEBUG
                var typeInfo         = typeof(T);
                var typeInfoBaseType = typeInfo.BaseType;
                if (typeInfoBaseType.GenericTypeArguments[0] != typeInfo)
                    throw new ECSException("Invalid Group Tag declared");

                GroupNamesMap.idToName[(uint)group] = $"Compound: {typeInfo.Name} ID {(uint)group}";
#endif
 GroupHashMap.RegisterGroup(BuildGroup,
                $"Compound: {typeof(T).FullName}");
                }
        }

        //Each time a new combination of group tags is found a new group is added.
        internal static void Add(ExclusiveGroupStruct group)
        {
            for (var i = 0; i < _Groups.count; ++i)
                if (_Groups[i] == group)
                    throw new Exception("temporary must be transformed in unit test");

            _Groups.Add(group);
            _GroupsHashSet.Add(group);
        }

        public static bool Includes(ExclusiveGroupStruct @group) { return _GroupsHashSet.Contains(@group); }
    }
}