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
        bool                MappedRemove(EGID entityID);
        ITypeSafeDictionary CreateIndexedDictionary();
        EGIDEnumerator      EntityIDS();
        void                AddCapacity(int capacity);

        void Fill(FasterList<IHandleEntityViewEngineAbstracted> enginesForEntityView);
    }

    public struct EGIDEnumerator:IEnumerable, IEnumerator
    {
        Dictionary<long, int>.Enumerator _keysEnumerator;

        public EGIDEnumerator(Dictionary<long, int> mappedIndices)
        {
            _keysEnumerator = mappedIndices.GetEnumerator();
        }

        public bool MoveNext()
        {
            return _keysEnumerator.MoveNext();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public object Current { get { return new EGID(_keysEnumerator.Current.Key);} }
        public IEnumerator GetEnumerator()
        {
            return this;
        }
    }

    class TypeSafeFasterListForECS<T> : FasterList<T> where T : IEntityData
    {
        protected TypeSafeFasterListForECS()
        {
            _mappedIndices = new Dictionary<long, int>();
        }

        protected TypeSafeFasterListForECS(int size) : base(size)
        {
            _mappedIndices = new Dictionary<long, int>();
        }

        public bool MappedRemove(EGID entityID)
        {
            var index = _mappedIndices[entityID.GID];

            Check.Assert(entityID.GID == this[index].ID.GID,
                         "Something went wrong with the Svelto.ECS code, please contact the author");

            _mappedIndices.Remove(entityID.GID);

            if (UnorderedRemoveAt(index))
            {
                _mappedIndices[this[index].ID.GID] = index;
            }

            return Count > 0;
        }

        public void AddRange(ITypeSafeList entityViewListValue)
        {
            var index = Count;

            base.AddRange(entityViewListValue as FasterList<T>);

            for (var i = index; i < Count; ++i)
            {
                try
                {
                    _mappedIndices.Add(this[i].ID.GID, i);
                }
                catch (Exception e)
                {
                    throw new TypeSafeFasterListForECSException(e);
                }
            }
        }

        public new void Add(T entityView)
        {
            var index = Count;

            base.Add(entityView);

            try
            {
                _mappedIndices.Add(entityView.ID.GID, index);
            }
            catch (Exception e)
            {
                throw new TypeSafeFasterListForECSException(e);
            }
        }

        public void AddCapacity(int capacity)
        {
            if (ToArrayFast().Length < Count + capacity)
                Resize(Count + capacity);
        }

        public int GetIndexFromID(EGID entityID)
        {
            return _mappedIndices[entityID.GID];
        }
        
        public EGIDEnumerator EntityIDS()
        {
            return new EGIDEnumerator(_mappedIndices);
        }

        readonly Dictionary<long, int> _mappedIndices;
    }

    class TypeSafeFasterListForECSForStructs<T> : TypeSafeFasterListForECS<T>, ITypeSafeList where T:struct, IEntityData
    {
        public TypeSafeFasterListForECSForStructs(int size) : base(size)
        {}

        public TypeSafeFasterListForECSForStructs()
        {}

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
    
    class TypeSafeFasterListForECSForClasses<T> : TypeSafeFasterListForECS<T>, ITypeSafeList where T:IEntityData, new()
    {
        public TypeSafeFasterListForECSForClasses(int size) : base(size)
        {}

        public TypeSafeFasterListForECSForClasses()
        {}

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
            return new TypeSafeDictionaryForClass<T>();
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