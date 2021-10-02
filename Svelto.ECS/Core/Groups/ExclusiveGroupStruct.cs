using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    //the type doesn't implement IEqualityComparer, what implements it is a custom comparer
    public struct ExclusiveGroupStruct : IEquatable<ExclusiveGroupStruct>, IComparable<ExclusiveGroupStruct>
    {
        public static readonly ExclusiveGroupStruct Invalid = default; //must stay here because of Burst

        public ExclusiveGroupStruct(byte[] data, uint pos):this()
        {
            _id = (uint)(
                data[pos]
              | data[++pos] << 8
              | data[++pos] << 16
            );
            _bytemask = (byte) (data[++pos] << 24);

            DBC.ECS.Check.Ensure(_id < _globalId, "Invalid group ID deserialiased");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is ExclusiveGroupStruct other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return (int) _id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ExclusiveGroupStruct c1, ExclusiveGroupStruct c2)
        {
            return c1.Equals(c2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ExclusiveGroupStruct c1, ExclusiveGroupStruct c2)
        {
            return c1.Equals(c2) == false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ExclusiveGroupStruct other)
        {
            return other._id == _id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(ExclusiveGroupStruct other)
        {
            return other._id.CompareTo(_id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsEnabled()
        {
            return (_bytemask & (byte)ExclusiveGroupBitmask.DISABLED_BIT) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Disable()
        {
            _bytemask |= (byte)ExclusiveGroupBitmask.DISABLED_BIT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Enable()
        {
            _bytemask &= (byte)(~ExclusiveGroupBitmask.DISABLED_BIT);
        }

        public override string ToString()
        {
            return this.ToName();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator uint(ExclusiveGroupStruct groupStruct)
        {
            return groupStruct._id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ExclusiveGroupStruct operator+(ExclusiveGroupStruct a, uint b)
        {
            var group = new ExclusiveGroupStruct {_id = a._id + b};

            return @group;
        }

        internal static ExclusiveGroupStruct Generate(byte bitmask = 0)
        {
            ExclusiveGroupStruct groupStruct;

            groupStruct._id = _globalId;
            groupStruct._bytemask = bitmask;
            DBC.ECS.Check.Require(_globalId + 1 < ExclusiveGroup.MaxNumberOfExclusiveGroups, "too many exclusive groups created");
            _globalId++;

            return groupStruct;
        }

        internal ExclusiveGroupStruct(ExclusiveGroupStruct @group):this() { this = group; }

        /// <summary>
        /// Use this constructor to reserve N groups
        /// </summary>
        internal ExclusiveGroupStruct(ushort range):this()
        {
            _id = _globalId;
            DBC.ECS.Check.Require(_globalId + range < ExclusiveGroup.MaxNumberOfExclusiveGroups, "too many exclusive groups created");
            _globalId += range;
        }

        internal ExclusiveGroupStruct(uint groupID):this()
        {
            _id = groupID;
        }

        [FieldOffset(0)] uint _id;
        [FieldOffset(3)] byte _bytemask;

        static           uint _globalId = 1; //it starts from 1 because default EGID is considered not initialized value
    }
}