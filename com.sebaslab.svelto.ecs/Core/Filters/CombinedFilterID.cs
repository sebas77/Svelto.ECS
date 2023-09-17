using System;
using System.Runtime.CompilerServices;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public readonly struct CombinedFilterID
    {
        //filter (32) | contextID (16) | component type (16) 
        readonly long id;

        //a context ID is 16bit
        public FilterContextID contextID
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new FilterContextID((ushort)((id & 0xFFFF0000) >> 16));
        }

        public uint filterID
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (uint)(id >> 32);
        }

        public CombinedFilterID(int filterID, FilterContextID contextID)
        {
            id = (long)filterID << 32 | (long)contextID.id << 16;
        }
        
        public CombinedFilterID(uint filterID, FilterContextID contextID)
        {
            id = (long)filterID << 32 | (long)contextID.id << 16;
        }

        public static implicit operator CombinedFilterID((int filterID, FilterContextID contextID) data)
        {
            return new CombinedFilterID(data.filterID, data.contextID);
        }
    }

    readonly struct CombinedFilterComponentID: IEquatable<CombinedFilterComponentID>
    {
        //filter (32) | contextID (16) | component type (16) 
        internal readonly long id;

        //a context ID is 16bit
        public FilterContextID contextID
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return new FilterContextID((ushort)((id & 0xFFFF0000) >> 16));
            }
        }

        public uint filterID
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (uint)(id >> 32);
            }
        }
        
        public uint contextComponentID
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (uint)(id & 0xFFFF);
            }
        }

        public CombinedFilterComponentID(int filterID, FilterContextID contextID)
        {
            id = (long)filterID << 32 | (uint)contextID.id << 16;
        }

        public CombinedFilterComponentID(uint filterIdFilterId, FilterContextID filterIdContextId, ComponentID componentid)
        {
            id = (long)filterIdFilterId << 32 | (long)filterIdContextId.id << 16 | (long)(uint)componentid;
        }

        public bool Equals(CombinedFilterComponentID other)
        {
            return id == other.id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }

    public static class CombinedFilterIDExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CombinedFilterComponentID CombineComponent<T>(this CombinedFilterID filterID)
                where T : struct, _IInternalEntityComponent
        {
            var componentid = ComponentTypeID<T>.id;

            DBC.ECS.Check.Require(componentid < ushort.MaxValue, "too many component types registered, HOW :)");

            return new CombinedFilterComponentID(filterID.filterID, filterID.contextID, componentid);
        }
    }
}