using System;
using System.Collections;
using System.Collections.Generic;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct EntityCollection<T>
    {
        public EntityCollection(T[] array, uint count)
        {
            _array = array;
            _count = count;
        }

        public EntityIterator<T> GetEnumerator()
        {
            return new EntityIterator<T>(_array, _count);
        }

        readonly T[] _array;
        readonly uint _count;

        public struct EntityIterator<T> : IEnumerator<T>
        {
            public EntityIterator(T[] array, uint count) : this()
            {
                _array = array;
                _count = count;
                _index = -1;
            }

            public bool MoveNext()
            {
                return ++_index < _count;
            }

            public void Reset()
            {
                _index = -1;
            }

            public ref T Current => ref _array[_index];

            T IEnumerator<T>.Current => throw new NotImplementedException();
            object IEnumerator.Current => throw new NotImplementedException();

            public void Dispose()
            {
            }

            readonly T[] _array;
            readonly uint _count;
            int _index;
        }
    }

    public struct EntityCollections<T> where T : struct, IEntityStruct
    {
        public EntityCollections(IEntitiesDB db, ExclusiveGroup[] groups) : this()
        {
            _db = db;
            _groups = groups;
        }

        public EntityGroupsIterator<T> GetEnumerator()
        {
            return new EntityGroupsIterator<T>(_db, _groups);
        }

        readonly IEntitiesDB _db;
        readonly ExclusiveGroup[] _groups;

        public struct EntityGroupsIterator<T> : IEnumerator<T> where T : struct, IEntityStruct
        {
            public EntityGroupsIterator(IEntitiesDB db, ExclusiveGroup[] groups) : this()
            {
                _db = db;
                _groups = groups;
                _indexGroup = -1;
                _index = -1;
            }

            public bool MoveNext()
            {
                while (_index + 1 >= _count && ++_indexGroup < _groups.Length)
                {
                    _index = -1;
                    _array = _db.QueryEntities<T>(_groups[_indexGroup], out _count);
                }

                return ++_index < _count;
            }

            public void Reset()
            {
                _index = -1;
                _indexGroup = -1;
                _array = _db.QueryEntities<T>(_groups[0], out _count);
            }

            public ref T Current => ref _array[_index];

            T IEnumerator<T>.Current => throw new NotImplementedException();
            object IEnumerator.Current => throw new NotImplementedException();

            public void Dispose() {}

            readonly IEntitiesDB _db;
            readonly ExclusiveGroup[] _groups;

            T[] _array;
            uint _count;
            int _index;
            int _indexGroup;
        }
    }

    public struct EntityCollections<T1, T2> where T1 : struct, IEntityStruct where T2 : struct, IEntityStruct
    {
        public EntityCollections(IEntitiesDB db, ExclusiveGroup[] groups) : this()
        {
            _db = db;
            _groups = groups;
        }

        public EntityGroupsIterator<T1, T2> GetEnumerator()
        {
            return new EntityGroupsIterator<T1, T2>(_db, _groups);
        }

        readonly IEntitiesDB _db;
        readonly ExclusiveGroup[] _groups;

        public struct EntityGroupsIterator<T1, T2> : IEnumerator<EntityGroupsIterator<T1, T2>.ValueRef>
            where T1 : struct, IEntityStruct
            where T2 : struct, IEntityStruct
        {
            public EntityGroupsIterator(IEntitiesDB db, ExclusiveGroup[] groups) : this()
            {
                _db = db;
                _groups = groups;
                _indexGroup = -1;
                _index = -1;
            }

            public bool MoveNext()
            {
                while (_index + 1 >= _count && ++_indexGroup < _groups.Length)
                {
                    _index = -1;
                    _array1 = _db.QueryEntities<T1>(_groups[_indexGroup], out _count);
                    _array2 = _db.QueryEntities<T2>(_groups[_indexGroup], out var count1);

#if DEBUG && !PROFILER
                    if (_count != count1)
                        throw new ECSException("number of entities in group doesn't match");
#endif
                }

                return ++_index < _count;
            }

            public void Reset()
            {
                _index = -1;
                _indexGroup = -1;
                
                _array1 = _db.QueryEntities<T1>(_groups[0], out _count);
                _array2 = _db.QueryEntities<T2>(_groups[0], out var count1);
#if DEBUG && !PROFILER
                if (_count != count1)
                    throw new ECSException("number of entities in group doesn't match");
#endif
            }

            public ValueRef Current => new ValueRef(_array1, _array2, (uint) _index);

            ValueRef IEnumerator<ValueRef>.Current => throw new NotImplementedException();
            object IEnumerator.Current => throw new NotImplementedException();

            public void Dispose() {}

            readonly IEntitiesDB _db;
            readonly ExclusiveGroup[] _groups;
            uint _count;
            int _index;
            int _indexGroup;
            T2[] _array2;
            T1[] _array1;

            public struct ValueRef
            {
                readonly T1[] array1;
                readonly T2[] array2;

                readonly uint index;

                public ValueRef(T1[] entity1, T2[] entity2, uint i) : this()
                {
                    array1 = entity1;
                    array2 = entity2;
                    index = i;
                }

                public ref T1 Item1 => ref array1[index];
                public ref T2 Item2 => ref array2[index];
            }
        }
    }
}