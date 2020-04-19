using Svelto.DataStructures;

namespace Svelto.ECS
{
    public struct NativeGroupsEnumerable<T1, T2, T3, T4>
        where T1 : unmanaged, IEntityComponent where T2 : unmanaged, IEntityComponent 
        where T3 : unmanaged, IEntityComponent where T4 : unmanaged, IEntityComponent
    {
        readonly EntitiesDB       _db;
        readonly ExclusiveGroupStruct[] _groups;

        public NativeGroupsEnumerable(EntitiesDB db, ExclusiveGroupStruct[] groups)
        {
            _db     = db;
            _groups = groups;
        }

        public struct NativeGroupsIterator
        {
            public NativeGroupsIterator(EntitiesDB db, ExclusiveGroupStruct[] groups) : this()
            {
                _groups     = groups;
                _indexGroup = -1;
                _entitiesDB = db;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.Length)
                {
                    var entityCollection = _entitiesDB.QueryEntities<T1, T2, T3>(_groups[_indexGroup]);
                    if (entityCollection.count == 0) continue;
                    var entityCollection2 = _entitiesDB.QueryEntities<T4>(_groups[_indexGroup]);
                    if (entityCollection2.count == 0) continue;
                    
                    DBC.ECS.Check.Assert(entityCollection.count == entityCollection2.count, "congratulation, you found a bug in Svelto, please report it");

                    BT<NB<T1>, NB<T2>, NB<T3>> array = entityCollection.ToNativeBuffers<T1, T2, T3>();
                    NB<T4> array2 = entityCollection2.ToNativeBuffer<T4>();
                    _array= new BT<NB<T1>, NB<T2>, NB<T3>, NB<T4>>(array.buffer1, array.buffer2, array.buffer3, array2, entityCollection.count);
                    break;
                }

                return _indexGroup < _groups.Length;
            }

            public void Reset() { _indexGroup = -1; }

            public BT<NB<T1>, NB<T2>, NB<T3>, NB<T4>> Current => _array;

            readonly ExclusiveGroupStruct[] _groups;

            int                        _indexGroup;
            BT<NB<T1>, NB<T2>, NB<T3>, NB<T4>> _array;
            readonly EntitiesDB        _entitiesDB;
        }

        public NativeGroupsIterator GetEnumerator() { return new NativeGroupsIterator(_db, _groups); }
    }
    
    public struct NativeGroupsEnumerable<T1, T2, T3>
        where T1 : unmanaged, IEntityComponent where T2 : unmanaged, IEntityComponent where T3 : unmanaged, IEntityComponent
    {
        readonly EntitiesDB       _db;
        readonly ExclusiveGroupStruct[] _groups;

        public NativeGroupsEnumerable(EntitiesDB db, ExclusiveGroupStruct[] groups)
        {
            _db     = db;
            _groups = groups;
        }

        public struct NativeGroupsIterator
        {
            public NativeGroupsIterator(EntitiesDB db, ExclusiveGroupStruct[] groups) : this()
            {
                _groups     = groups;
                _indexGroup = -1;
                _entitiesDB = db;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.Length)
                {
                    var entityCollection = _entitiesDB.QueryEntities<T1, T2, T3>(_groups[_indexGroup]);
                    if (entityCollection.count == 0) continue;

                    _array = entityCollection.ToNativeBuffers<T1, T2, T3>();
                    break;
                }

                return _indexGroup < _groups.Length;
            }

            public void Reset() { _indexGroup = -1; }

            public BT<NB<T1>, NB<T2>, NB<T3>> Current => _array;

            readonly ExclusiveGroupStruct[] _groups;

            int                        _indexGroup;
            BT<NB<T1>, NB<T2>, NB<T3>> _array;
            readonly EntitiesDB        _entitiesDB;
        }

        public NativeGroupsIterator GetEnumerator() { return new NativeGroupsIterator(_db, _groups); }
    }

    public struct NativeGroupsEnumerable<T1, T2> where T1 : unmanaged, IEntityComponent where T2 : unmanaged, IEntityComponent
    {
        public NativeGroupsEnumerable(EntitiesDB db, ExclusiveGroupStruct[] groups)
        {
            _db = db;
            _groups = groups;
        }

        public struct NativeGroupsIterator
        {
            public NativeGroupsIterator(EntitiesDB db, ExclusiveGroupStruct[] groups) : this()
            {
                _db = db;
                _groups = groups;
                _indexGroup = -1;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.Length)
                {
                    var entityCollection = _db.QueryEntities<T1, T2>(_groups[_indexGroup]);
                    if (entityCollection.count == 0) continue;

                    _array = entityCollection.ToNativeBuffers<T1, T2>();
                    break;
                }

                return _indexGroup < _groups.Length;
            }

            public void Reset()
            {
                _indexGroup = -1;
            }

            public BT<NB<T1>, NB<T2>> Current => _array;

            readonly EntitiesDB       _db;
            readonly ExclusiveGroupStruct[] _groups;

            int                _indexGroup;
            BT<NB<T1>, NB<T2>> _array;
        }

        public NativeGroupsIterator GetEnumerator()
        {
            return new NativeGroupsIterator(_db, _groups);
        }

        readonly EntitiesDB       _db;
        readonly ExclusiveGroupStruct[] _groups;
    }
    
    public struct NativeGroupsEnumerable<T1> where T1 : unmanaged, IEntityComponent
    {
        public NativeGroupsEnumerable(EntitiesDB db, ExclusiveGroupStruct[] groups)
        {
            _db     = db;
            _groups = groups;
        }

        public struct NativeGroupsIterator
        {
            public NativeGroupsIterator(EntitiesDB db, ExclusiveGroupStruct[] groups) : this()
            {
                _db         = db;
                _groups     = groups;
                _indexGroup = -1;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.Length)
                {
                    var entityCollection = _db.QueryEntities<T1>(_groups[_indexGroup]);
                    if (entityCollection.count == 0) continue;

                    _array = entityCollection.ToNativeBuffer<T1>();
                    break;
                }

                return _indexGroup < _groups.Length;
            }

            public void Reset()
            {
                _indexGroup = -1;
            }

            public NB<T1> Current => _array;

            readonly EntitiesDB       _db;
            readonly ExclusiveGroupStruct[] _groups;

            int                _indexGroup;
            NB<T1> _array;
        }

        public NativeGroupsIterator GetEnumerator()
        {
            return new NativeGroupsIterator(_db, _groups);
        }

        readonly EntitiesDB       _db;
        readonly ExclusiveGroupStruct[] _groups;
    }
}