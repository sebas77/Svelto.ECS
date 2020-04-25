using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct QueryGroups
    {
        public readonly FasterList<ExclusiveGroupStruct> groups;
        
        public QueryGroups(FasterDictionary<uint, ITypeSafeDictionary> findGroups)
        {
            var findGroupsCount = findGroups.count;
            groups = new FasterList<ExclusiveGroupStruct>(findGroupsCount);
            foreach (var keyvalue in findGroups)
            {
                groups.Add(new ExclusiveGroupStruct(keyvalue.Key));
            }
        }

        public QueryGroups Except(ExclusiveGroupStruct[] groupsToIgnore)
        {
            var groupsCount = groups.count;

            for (int i = 0; i < groupsToIgnore.Length; i++)
            {
                for (int j = 0; j < groupsCount; j++)
                    if (groupsToIgnore[i] == groups[j])
                    {
                        groups.UnorderedRemoveAt(j);
                        j--;
                        groupsCount--;
                    }
            }

            return this;
        }
    }
}