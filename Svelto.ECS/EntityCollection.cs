using System;
using System.Collections;
using System.Collections.Generic;

namespace Svelto.ECS
{
    public struct EntityCollection<T>
    {
        public EntityCollection(T[] array, uint count)
        {
            _array = array;
            _count = count;
        }

        public EntityIterator GetEnumerator()
        {
            return new EntityIterator(_array, _count);
        }

        readonly T[] _array;
        readonly uint _count;

        public struct EntityIterator : IEnumerator<T>
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

            public void Dispose()  {}

            readonly T[] _array;
            readonly uint _count;
            int _index;
        }
    }
    
    public struct EntityCollection<T1, T2>
    {
        public EntityCollection(in (T1[], T2[]) array, uint count)
        {
            _array = array;
            _count = count;
        }

        public EntityIterator GetEnumerator()
        {
            return new EntityIterator(_array, _count);
        }

        readonly (T1[], T2[])  _array;
        readonly uint _count;

        public struct EntityIterator : IEnumerator<ValueRef<T1, T2>>
        {
            public EntityIterator((T1[], T2[]) array, uint count) : this()
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

            public ValueRef<T1, T2> Current => new ValueRef<T1, T2>(_array, (uint) _index);

            ValueRef<T1, T2> IEnumerator<ValueRef<T1, T2>>. Current => throw new NotImplementedException();
            object IEnumerator.Current => throw new NotImplementedException();

            public void Dispose()  {}

            readonly (T1[], T2[])  _array;
            readonly uint _count;
            int           _index;
        }
    }
    
    public struct EntityCollection<T1, T2, T3>
    {
        public EntityCollection(in (T1[], T2[], T3[]) array, uint count)
        {
            _array = array;
            _count = count;
        }

        public EntityIterator GetEnumerator()
        {
            return new EntityIterator(_array, _count);
        }

        readonly (T1[], T2[], T3[]) _array;
        readonly uint         _count;

        public struct EntityIterator : IEnumerator<ValueRef<T1, T2, T3>>
        {
            public EntityIterator((T1[], T2[], T3[]) array, uint count) : this()
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

            public ValueRef<T1, T2, T3> Current => new ValueRef<T1, T2, T3>(_array, (uint) _index);

            ValueRef<T1, T2, T3> IEnumerator<ValueRef<T1, T2, T3>>.Current => throw new NotImplementedException();
            object IEnumerator.                                    Current => throw new NotImplementedException();

            public void Dispose()  {}

            readonly (T1[], T2[], T3[]) _array;
            readonly uint               _count;
            int                         _index;
        }
    }
    
    public struct EntityCollections<T> where T : struct, IEntityStruct
    {
        public EntityCollections(IEntitiesDB db, ExclusiveGroup[] groups) : this()
        {
            _db = db;
            _groups = groups;
        }

        public EntityGroupsIterator GetEnumerator()
        {
            return new EntityGroupsIterator(_db, _groups);
        }

        readonly IEntitiesDB _db;
        readonly ExclusiveGroup[] _groups;

        public struct EntityGroupsIterator : IEnumerator<T>
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
                _count = 0;
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

        public EntityGroupsIterator GetEnumerator()
        {
            return new EntityGroupsIterator(_db, _groups);
        }

        readonly IEntitiesDB _db;
        readonly ExclusiveGroup[] _groups;

        public struct EntityGroupsIterator : IEnumerator<ValueRef<T1, T2>>
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
                    var array1 = _db.QueryEntities<T1>(_groups[_indexGroup], out _count);
                    var array2 = _db.QueryEntities<T2>(_groups[_indexGroup], out var count1);
                    _array = (array1, array2);

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

                var array1 = _db.QueryEntities<T1>(_groups[0], out _count);
                var array2 = _db.QueryEntities<T2>(_groups[0], out var count1);
                _array = (array1, array2);
#if DEBUG && !PROFILER
                if (_count != count1)
                    throw new ECSException("number of entities in group doesn't match");
#endif
            }

            public ValueRef<T1, T2> Current
            {
                get
                {
                    var valueRef = new ValueRef<T1, T2>(_array, (uint) _index);
                    return valueRef;
                }
            }

            ValueRef<T1, T2> IEnumerator<ValueRef<T1, T2>>.Current => throw new NotImplementedException();
            object IEnumerator.Current => throw new NotImplementedException();

            public void Dispose() {}

            readonly IEntitiesDB      _db;
            readonly ExclusiveGroup[] _groups;
            uint                      _count;
            int                       _index;
            int                       _indexGroup;
            (T1[], T2[])              _array;
        }
    }
    
    public struct ValueRef<T1, T2>
    {
        readonly (T1[], T2[]) array;

        readonly uint index;

        public ValueRef(in (T1[], T2[]) entity1, uint i)
        {
            array = entity1;
            index = i;
        }

        public ref T1 entityStructA => ref array.Item1[index];
        public ref T2 entityStructB => ref array.Item2[index];
    }
    
    public struct ValueRef<T1, T2, T3>
    {
        readonly (T1[], T2[], T3[]) array;

        readonly uint index;

        public ValueRef(in (T1[], T2[], T3[]) entity1, uint i)
        {
            array = entity1;
            index = i;
        }

        public ref T1 entityStructA => ref array.Item1[index];
        public ref T2 entityStructB => ref array.Item2[index];
        public ref T3 entityStructC => ref array.Item3[index];

    }
}
