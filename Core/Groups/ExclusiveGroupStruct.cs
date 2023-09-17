using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
#pragma warning disable CS0660, CS0661

namespace Svelto.ECS
{
    [DebuggerDisplay("{ToString()}")]
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    //the type doesn't implement IEqualityComparer, what implements it is a custom comparer
    public readonly struct ExclusiveGroupStruct : IEquatable<ExclusiveGroupStruct>, IComparable<ExclusiveGroupStruct>
    {
        public static readonly ExclusiveGroupStruct Invalid; //must stay here because of Burst

        public ExclusiveGroupStruct(byte[] data, uint pos):this()
        {
            _idInternal = (uint)(
                data[pos]
              | data[++pos] << 8
              | data[++pos] << 16
            );
            _bytemask = (byte) (data[++pos] << 24);

            DBC.ECS.Check.Ensure(id < _globalId, "Invalid group ID deserialiased");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return (int) id;
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
#if DEBUG && !PROFILE_SVELTO
            if ((other.id != id || other._bytemask == _bytemask) == false)
                throw new ECSException(
                    "if the groups are correctly initialised, two groups with the same ID and different bitmask cannot exist");
#endif            
            return other.id == id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(ExclusiveGroupStruct other)
        {
            return other.id.CompareTo(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEnabled()
        {
            return (_bytemask & (byte)ExclusiveGroupBitmask.DISABLED_BIT) == 0;
        }

        public override string ToString()
        {
            return this.ToName();
        }
        
        public bool isInvalid => this == Invalid;
        public uint id        => _idInternal & 0xFFFFFF;

        public uint ToIDAndBitmask() => _idInternal;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ExclusiveGroupStruct operator+(ExclusiveGroupStruct a, uint b)
        {
            var aID   = a.id + b;
#if DEBUG && !PROFILE_SVELTO           
            if (aID >= 0xFFFFFF)
                throw new IndexOutOfRangeException();
#endif
            var group = new ExclusiveGroupStruct(aID);
 
            return @group;
        }
        
        internal static ExclusiveGroupStruct Generate()
        {
            var newValue = Interlocked.Increment(ref _staticGlobalID);
            
            ExclusiveGroupStruct groupStruct = new ExclusiveGroupStruct((uint) newValue - (uint) 1);
            
            DBC.ECS.Check.Require(_globalId < ExclusiveGroup.MaxNumberOfExclusiveGroups, "too many exclusive groups created");

            return groupStruct;
        }

        internal static ExclusiveGroupStruct Generate(byte bitmask)
        {
            var newValue = Interlocked.Increment(ref _staticGlobalID);
            
            ExclusiveGroupStruct groupStruct = new ExclusiveGroupStruct((uint) newValue - (uint) 1, bitmask);
            
            DBC.ECS.Check.Require(_globalId < ExclusiveGroup.MaxNumberOfExclusiveGroups, "too many exclusive groups created");

            return groupStruct;
        }

        /// <summary>
        /// Use this to reserve N groups. We of course assign the current ID and then increment the index
        /// by range so that the next reserved index will take the range in consideration. This method is used
        /// internally by ExclusiveGroup.
        /// </summary>
        internal static ExclusiveGroupStruct GenerateWithRange(ushort range)
        {
            var newValue = Interlocked.Add(ref _staticGlobalID, (int)range);
            
            ExclusiveGroupStruct groupStruct = new ExclusiveGroupStruct((uint)newValue - (uint)range);
            
            DBC.ECS.Check.Require(_globalId < ExclusiveGroup.MaxNumberOfExclusiveGroups, "too many exclusive groups created");

            return groupStruct;
        }
        
        public static ExclusiveGroupStruct GenerateWithRange(ushort range, byte bitmask)
        {
            var newValue = Interlocked.Add(ref _staticGlobalID, (int)range);
            
            ExclusiveGroupStruct groupStruct = new ExclusiveGroupStruct((uint)newValue - (uint)range, bitmask);
            
            DBC.ECS.Check.Require(_globalId < ExclusiveGroup.MaxNumberOfExclusiveGroups, "too many exclusive groups created");

            return groupStruct;
        }

        /// <summary>
        /// used internally only by the framework to convert uint in to groups. ID must be generated by the framework
        /// so only the framework can assure that this method is not being abused
        /// </summary>
        internal ExclusiveGroupStruct(uint groupID):this()
        {
#if DEBUG && !PROFILE_SVELTO          
            if (groupID >= 0xFFFFFF)
                throw new IndexOutOfRangeException();
#endif            
            _idInternal = groupID;
        }
        
        internal ExclusiveGroupStruct(uint groupID, byte bytemask):this()
        {
#if DEBUG && !PROFILE_SVELTO          
            if (groupID >= 0xFFFFFF)
                throw new IndexOutOfRangeException();
#endif            
            _idInternal = groupID;
            _bytemask   = bytemask;
        }
        
        static ExclusiveGroupStruct()
        {
            _staticGlobalID = 1;  
        }

        [FieldOffset(0)] readonly uint _idInternal;
        //byte mask can be used to add special flags to specific groups that can be checked for example when swapping groups
        //however at the moment we are not letting the user access it, because if we do so we should give access only to
        //4 bits are the other 4 bits will stay reserved for Svelto use (at the moment of writing using only the disable
        //bit)
        [FieldOffset(3)] readonly byte _bytemask;

        static int  _staticGlobalID;
        static uint _globalId => (uint) _staticGlobalID;
    }
}