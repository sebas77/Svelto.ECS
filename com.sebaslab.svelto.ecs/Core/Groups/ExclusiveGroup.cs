using System;
using System.Collections.Generic;

#pragma warning disable 660,661

namespace Svelto.ECS
{
    /// <summary>
    /// Exclusive Groups guarantee that the GroupID is unique.
    ///
    /// The best way to use it is like:
    ///
    /// public static class MyExclusiveGroups //(can be as many as you want)
    /// {
    ///     public static ExclusiveGroup MyExclusiveGroup1 = new ExclusiveGroup();
    ///
    ///     public static ExclusiveGroup[] GroupOfGroups = { MyExclusiveGroup1, ...}; //for each on this!
    /// }
    /// </summary>
    ///To debug it use in your debug window: Svelto.ECS.Debugger.EGID.GetGroupNameFromId(groupID)
    public sealed class ExclusiveGroup
    {
        public const uint MaxNumberOfExclusiveGroups = 2 << 20;

        public ExclusiveGroup()
        {
            _group = ExclusiveGroupStruct.Generate();
        }

        public ExclusiveGroup(string recognizeAs)
        {
            _group = ExclusiveGroupStruct.Generate();

            _knownGroups.Add(recognizeAs, _group);
        }
        
        public ExclusiveGroup(ExclusiveGroupBitmask bitmask)
        {
            _group = ExclusiveGroupStruct.Generate((byte) bitmask);
        }
        
        public ExclusiveGroup(ushort range)
        {
            _group = ExclusiveGroupStruct.GenerateWithRange(range);

            _range = range;
        }
        
        public ExclusiveGroup(ushort range, ExclusiveGroupBitmask bitmask)
        {
            _group = ExclusiveGroupStruct.GenerateWithRange(range, (byte)bitmask);
#if DEBUG && !PROFILE_SVELTO
            _range = range;
#endif
        }

        public static implicit operator ExclusiveGroupStruct(ExclusiveGroup group)
        {
            return group._group;
        }

        public static ExclusiveGroupStruct operator+(ExclusiveGroup @group, uint b)
        {
#if DEBUG && !PROFILE_SVELTO
            if (@group._range == 0)
                throw new ECSException($"Adding values to a not ranged ExclusiveGroup: {@group.id}");
            if (b >= @group._range)
                throw new ECSException($"Using out of range group: {@group.id} + {b}");
#endif
            return group._group + b;
        }
        
        public uint id => _group.id;

        //todo document the use case for this method. I may honestly set this as a deprecated as it's original scenario is probably not valid anymore
        public static ExclusiveGroupStruct Search(string holderGroupName)
        {
            if (_knownGroups.ContainsKey(holderGroupName) == false)
                throw new Exception("Named Group Not Found ".FastConcat(holderGroupName));

            return _knownGroups[holderGroupName];
        }

        public override string ToString()
        {
            return _group.ToString();
        }

        static readonly Dictionary<string, ExclusiveGroupStruct> _knownGroups =
            new Dictionary<string, ExclusiveGroupStruct>();

        internal readonly ushort _range;

        readonly ExclusiveGroupStruct _group;
    }
}