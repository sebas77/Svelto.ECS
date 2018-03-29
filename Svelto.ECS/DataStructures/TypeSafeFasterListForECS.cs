using System;
using System.Collections;
using System.Collections.Generic;
using DBC;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeList : IEnumerable
    {
        bool isQueryiableEntityView { get; }
        void AddRange(ITypeSafeList entityViewListValue);

        ITypeSafeList       Create();
        bool                MappedRemove(int entityID);
        ITypeSafeDictionary CreateIndexedDictionary();
        IEntityView[]       ToArrayFast(out int count);
        void                ReserveCapacity(int capacity);

        void Fill(FasterList<IHandleEntityViewEngineAbstracted> enginesForEntityView);
    }

    class TypeSafeFasterListForECS<T> : FasterList<T> where T : IEntityView
    {
        protected TypeSafeFasterListForECS()
        {
            _mappedIndices = new Dictionary<int, int>();
        }

        protected TypeSafeFasterListForECS(int size) : base(size)
        {
            _mappedIndices = new Dictionary<int, int>();
        }

        public bool MappedRemove(int entityID)
        {
            var index = _mappedIndices[entityID];

            Check.Assert(entityID == this[index].ID,
                         "Something went wrong with the Svelto.ECS code, please contact the author");

            _mappedIndices.Remove(entityID);

            if (UnorderedRemoveAt(index))
                _mappedIndices[this[index].ID] = index;

            return Count > 0;
        }

        public void AddRange(ITypeSafeList entityViewListValue)
        {
            var index = Count;

            base.AddRange(entityViewListValue as FasterList<T>);

            for (var i = index; i < Count; ++i)
                _mappedIndices[this[i].ID] = i;
        }

        public new void Add(T entityView)
        {
            var index = Count;

            base.Add(entityView);

            _mappedIndices[entityView.ID] = index;
        }

        public void ReserveCapacity(int capacity)
        {
            if (ToArrayFast().Length < capacity)
                Resize(capacity);
        }

        public int GetIndexFromID(int entityID)
        {
            return _mappedIndices[entityID];
        }

        readonly Dictionary<int, int> _mappedIndices;
    }

    class TypeSafeFasterListForECSForStructs<T> : TypeSafeFasterListForECS<T>, ITypeSafeList where T:struct, IEntityView
    {
        public TypeSafeFasterListForECSForStructs(int size) : base(size)
        {
        }

        public TypeSafeFasterListForECSForStructs()
        {
        }

        public ITypeSafeList Create()
        {
            return new TypeSafeFasterListForECSForStructs<T>();
        }

        public bool isQueryiableEntityView
        {
            get { return false; }
        }

        public ITypeSafeDictionary CreateIndexedDictionary()
        {
            throw new NotImplementedException();
        }

        public IEntityView[] ToArrayFast(out int count)
        {
            throw new NotImplementedException();
        }

        public void Fill(FasterList<IHandleEntityViewEngineAbstracted> enginesForEntityView)
        {
            var thisfastList = NoVirt.ToArrayFast(this);
            for (int i = 0; i < Count; i++)
            {
                int count;
                var fastList = FasterList<IHandleEntityViewEngineAbstracted>.NoVirt.ToArrayFast(enginesForEntityView, out count);
                for (int j = 0; j < count; j++)
                {
#if ENGINE_PROFILER_ENABLED
                    EngineProfiler.MonitorAddDuration<T>(fastList[j], entityView);
#else
                    (fastList[j] as IHandleEntityStructEngine<T>).Add(ref thisfastList[j]);
#endif
                }
            }
        }

        public ITypeSafeList Create(int size)
        {
            return new TypeSafeFasterListForECSForStructs<T>(size);
        }
    }
    
    class TypeSafeFasterListForECSForClasses<T> : TypeSafeFasterListForECS<T>, ITypeSafeList where T:class, IEntityView, new()
    {
        public TypeSafeFasterListForECSForClasses(int size) : base(size)
        {
        }

        public TypeSafeFasterListForECSForClasses()
        {
        }

        public ITypeSafeList Create()
        {
            return new TypeSafeFasterListForECSForClasses<T>();
        }

        public bool isQueryiableEntityView
        {
            get { return true; }
        }

        public ITypeSafeDictionary CreateIndexedDictionary()
        {
            return new TypeSafeDictionary<T>();
        }

        public IEntityView[] ToArrayFast(out int count)
        {
            count = Count;

            return ToArrayFast();
        }

        public void Fill(FasterList<IHandleEntityViewEngineAbstracted> enginesForEntityView)
        {
            var thisfastList = NoVirt.ToArrayFast(this);
            for (int i = 0; i < Count; i++)
            {
                int count;
                var fastList = FasterList<IHandleEntityViewEngineAbstracted>.NoVirt.ToArrayFast(enginesForEntityView, out count);
                for (int j = 0; j < count; j++)
                {
#if ENGINE_PROFILER_ENABLED
                    EngineProfiler.MonitorAddDuration(fastList[j], entityView);
#else
                    (fastList[j] as IHandleEntityStructEngine<T>).Add(ref thisfastList[j]);
#endif
                }
            }
        }

        public ITypeSafeList Create(int size)
        {
            return new TypeSafeFasterListForECSForClasses<T>(size);
        }
    }
}