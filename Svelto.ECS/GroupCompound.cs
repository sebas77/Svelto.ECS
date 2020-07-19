using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public abstract class GroupCompound<G1, G2, G3>
        where G1 : GroupTag<G1> where G2 : GroupTag<G2> where G3 : GroupTag<G3>
    {
        static readonly FasterList<ExclusiveGroupStruct> _Groups;
        
        public static FasterReadOnlyList<ExclusiveGroupStruct> Groups => new FasterReadOnlyList<ExclusiveGroupStruct>(_Groups);

        static GroupCompound()
        {
            if ((_Groups = GroupCompound<G3, G1, G2>._Groups) == null)
            if ((_Groups = GroupCompound<G2, G3, G1>._Groups) == null)
            if ((_Groups = GroupCompound<G3, G2, G1>._Groups) == null)
            if ((_Groups = GroupCompound<G1, G3, G2>._Groups) == null)
            if ((_Groups = GroupCompound<G2, G1, G3>._Groups) == null)
            {
                _Groups = new FasterList<ExclusiveGroupStruct>(1);

                var Group = new ExclusiveGroup();
                _Groups.Add(Group);
                    
                GroupCompound<G1, G2>.Add(Group); //<G1/G2> and <G2/G1> must share the same array
                GroupCompound<G1, G3>.Add(Group);
                GroupCompound<G2, G3>.Add(Group);
                
                //This is done here to be sure that the group is added once per group tag
                //(if done inside the previous group compound it would be added multiple times)
                GroupTag<G1>.Add(Group);
                GroupTag<G2>.Add(Group);
                GroupTag<G3>.Add(Group);
                
#if DEBUG
                GroupMap.idToName[(uint) Group] = $"Compound: {typeof(G1).Name}-{typeof(G2).Name}-{typeof(G3).Name}";
#endif 
            }
        }
        
        public static void Add(ExclusiveGroupStruct @group)
        {
            for (int i = 0; i < _Groups.count; ++i)
                if (_Groups[i] == group)
                    throw new Exception("temporary must be transformed in unit test");
            
            _Groups.Add(group);
            
          //  GroupCompound<G1, G2, G3>._Groups = _Groups;
        }

        public static ExclusiveGroupStruct BuildGroup => new ExclusiveGroupStruct(_Groups[0]);
    }

    public abstract class GroupCompound<G1, G2> where G1 : GroupTag<G1> where G2 : GroupTag<G2>
    {
        static FasterList<ExclusiveGroupStruct> _Groups; 
        public static FasterReadOnlyList<ExclusiveGroupStruct> Groups => new FasterReadOnlyList<ExclusiveGroupStruct>(_Groups);

        static GroupCompound()
        {
            _Groups = GroupCompound<G2, G1>._Groups;
            
            if (_Groups == null)
            {
                _Groups = new FasterList<ExclusiveGroupStruct>(1);
                var Group = new ExclusiveGroup();
                _Groups.Add(Group);
                
                //every abstract group preemptively adds this group, it may or may not be empty in future
                GroupTag<G1>.Add(Group);
                GroupTag<G2>.Add(Group);
                
#if DEBUG        
                GroupMap.idToName[(uint) Group] = $"Compound: {typeof(G1).Name}-{typeof(G2).Name}";
#endif 
            }
        } 

        public static ExclusiveGroupStruct BuildGroup => new ExclusiveGroupStruct(_Groups[0]);

        public static void Add(ExclusiveGroupStruct @group)
        {
            for (int i = 0; i < _Groups.count; ++i)
                if (_Groups[i] == group)
                    throw new Exception("temporary must be transformed in unit test");
            
            _Groups.Add(group);
            
            //unit test this to check if it's necessary
          //  GroupCompound<G2, G1>._Groups = _Groups;
        }
    }

    //A Group Tag holds initially just a group, itself. However the number of groups can grow with the number of
    //combinations of GroupTags including this one. This because a GroupTag is an adjective and different entities
    //can use the same adjective together with other ones. However since I need to be able to iterate over all the
    //groups with the same adjective, a group tag needs to hold all the groups sharing it.
    public abstract class GroupTag<T> where T : GroupTag<T>
    {
        static FasterList<ExclusiveGroupStruct> _Groups = new FasterList<ExclusiveGroupStruct>(1);
        
        public static FasterReadOnlyList<ExclusiveGroupStruct> Groups => new FasterReadOnlyList<ExclusiveGroupStruct>(_Groups);

        static GroupTag()
        {
            _Groups.Add(new ExclusiveGroup());
        }

        //Each time a new combination of group tags is found a new group is added.
        internal static void Add(ExclusiveGroupStruct @group)
        {
            for (int i = 0; i < _Groups.count; ++i)
                if (_Groups[i] == group)
                    throw new Exception("temporary must be transformed in unit test");

            _Groups.Add(group);
#if DEBUG                    
            GroupMap.idToName[(uint) group] = $"Compound: {typeof(T).Name}";
#endif            
        }

        public static ExclusiveGroupStruct BuildGroup => new ExclusiveGroupStruct(_Groups[0]);
    }
}
