using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
        public const           uint                 MaxNumberOfExclusiveGroups = 2 << 20;

        public ExclusiveGroup(ExclusiveGroupBitmask bitmask = 0)
        {
            _group = ExclusiveGroupStruct.Generate((byte)bitmask);
        }

        public ExclusiveGroup(string recognizeAs, ExclusiveGroupBitmask bitmask = 0)
        {
            _group = ExclusiveGroupStruct.Generate((byte)bitmask);

            _knownGroups.Add(recognizeAs, _group);
        }

        public ExclusiveGroup(ushort range)
        {
            _group = new ExclusiveGroupStruct(range);
#if DEBUG
            _range = range;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Disable()
        {
            _group.Disable();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enable()
        {
            _group.Enable();
        }

        public static implicit operator ExclusiveGroupStruct(ExclusiveGroup group)
        {
            return group._group;
        }

        public static explicit operator uint(ExclusiveGroup group)
        {
            return (uint) @group._group;
        }

        public static ExclusiveGroupStruct operator+(ExclusiveGroup a, uint b)
        {
#if DEBUG
            if (a._range == 0)
                throw new ECSException($"Adding values to a not ranged ExclusiveGroup: {(uint)a}");
            if (b >= a._range)
                throw new ECSException($"Using out of range group: {(uint)a} + {b}");
#endif
            return a._group + b;
        }

        //todo document the use case for this method
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

        static readonly Dictionary<string, ExclusiveGroupStruct> _knownGroups = new Dictionary<string,
            ExclusiveGroupStruct>();

#if DEBUG
        readonly ushort _range;
#endif
        ExclusiveGroupStruct _group;
    }
}
