using System;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public readonly ref struct EntityCollections<T1, T2, T3, T4> where T1 : struct, IEntityComponent
                                                             where T2 : struct, IEntityComponent
                                                             where T3 : struct, IEntityComponent
                                                             where T4 : struct, IEntityComponent
    {
        public EntityCollections(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
        {
            _db     = db;
            _groups = groups;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityGroupsIterator GetEnumerator()
        {
            throw new NotImplementedException("tell seb to finish this one");
#pragma warning disable 162
            return new EntityGroupsIterator(_db, _groups);
#pragma warning restore 162
        }

        readonly EntitiesDB                               _db;
        readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        public ref struct EntityGroupsIterator
        {
            public EntityGroupsIterator(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _db         = db;
                _groups     = groups;
                _indexGroup = -1;
                _index      = -1;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (_index + 1 >= _count && ++_indexGroup < _groups.count)
                {
                    _index  = -1;
                    _array1 = _db.QueryEntities<T1, T2, T3>(_groups[_indexGroup]);
                    _count  = _array1.count;
                }

                return ++_index < _count;
            }

            public void Reset()
            {
                _index      = -1;
                _indexGroup = -1;

                _array1 = _db.QueryEntities<T1, T2, T3>(_groups[0]);
                _count  = _array1.count;
            }

            public ValueRef<T1, T2, T3> Current
            {
                get
                {
                    var valueRef = new ValueRef<T1, T2, T3>(_array1, (uint) _index);
                    return valueRef;
                }
            }

            readonly EntitiesDB                               _db;
            readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;
            uint                                              _count;
            int                                               _index;
            int                                               _indexGroup;

            EntityCollection<T1, T2, T3> _array1;
        }
    }

    public readonly ref struct EntityCollections<T1, T2, T3> where T1 : struct, IEntityComponent
                                                         where T2 : struct, IEntityComponent
                                                         where T3 : struct, IEntityComponent
    {
        public EntityCollections(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
        {
            _db     = db;
            _groups = groups;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityGroupsIterator GetEnumerator() { return new EntityGroupsIterator(_db, _groups); }

        readonly EntitiesDB                               _db;
        readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        public ref struct EntityGroupsIterator
        {
            public EntityGroupsIterator(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _db         = db;
                _groups     = groups;
                _indexGroup = -1;
                _index      = -1;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (_index + 1 >= _count && ++_indexGroup < _groups.count)
                {
                    _index  = -1;
                    _array1 = _db.QueryEntities<T1, T2, T3>(_groups[_indexGroup]);
                    _count  = _array1.count;
                }

                return ++_index < _count;
            }

            public void Reset()
            {
                _index      = -1;
                _indexGroup = -1;

                _array1 = _db.QueryEntities<T1, T2, T3>(_groups[0]);
                _count  = _array1.count;
            }

            public ValueRef<T1, T2, T3> Current
            {
                get
                {
                    var valueRef = new ValueRef<T1, T2, T3>(_array1, (uint) _index);
                    return valueRef;
                }
            }

            readonly EntitiesDB                               _db;
            readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;
            uint                                              _count;
            int                                               _index;
            int                                               _indexGroup;

            EntityCollection<T1, T2, T3> _array1;
        }
    }

    public readonly ref struct EntityCollections<T1, T2>
        where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent
    {
        public EntityCollections(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
        {
            _db     = db;
            _groups = groups;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityGroupsIterator GetEnumerator() { return new EntityGroupsIterator(_db, _groups); }

        readonly EntitiesDB                               _db;
        readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        public ref struct EntityGroupsIterator
        {
            public EntityGroupsIterator(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _db         = db;
                _groups     = groups;
                _indexGroup = -1;
                _index      = -1;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (_index + 1 >= _array1.count && ++_indexGroup < _groups.count)
                {
                    _index  = -1;
                    _array1 = _db.QueryEntities<T1, T2>(_groups[_indexGroup]);
                }

                return ++_index < _array1.count;
            }

            public void Reset()
            {
                _index      = -1;
                _indexGroup = -1;

                _array1 = _db.QueryEntities<T1, T2>(_groups[0]);
            }

            public ValueRef<T1, T2> Current
            {
                get
                {
                    var valueRef = new ValueRef<T1, T2>(_array1, (uint) _index);
                    return valueRef;
                }
            }

            readonly EntitiesDB                               _db;
            readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;
            int                                               _index;
            int                                               _indexGroup;

            EntityCollection<T1, T2> _array1;
        }
    }

    public readonly ref struct EntityCollections<T> where T : struct, IEntityComponent
    {
        public EntityCollections(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
        {
            _db     = db;
            _groups = groups;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityGroupsIterator GetEnumerator() { return new EntityGroupsIterator(_db, _groups); }

        readonly EntitiesDB                               _db;
        readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        public ref struct EntityGroupsIterator
        {
            public EntityGroupsIterator(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _db         = db;
                _groups     = groups;
                _indexGroup = -1;
                _index      = -1;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (_index + 1 >= _count && ++_indexGroup < _groups.count)
                {
                    _index = -1;
                    _array = _db.QueryEntities<T>(_groups[_indexGroup]);
                    _count = _array.count;
                }

                return ++_index < _count;
            }

            public void Reset()
            {
                _index      = -1;
                _indexGroup = -1;
                _count      = 0;
            }

            public ref T Current => ref _array[(uint) _index];

            readonly EntitiesDB                               _db;
            readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

            EntityCollection<T> _array;
            uint                _count;
            int                 _index;
            int                 _indexGroup;
        }
    }
}