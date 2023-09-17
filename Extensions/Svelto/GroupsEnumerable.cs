using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    /// <summary>
    ///     NOTE THESE ENUMERABLES EXIST TO AVOID BOILERPLATE CODE AS THEY SKIP 0 SIZED GROUPS
    ///     However if the normal pattern with the double foreach is used, this is not necessary
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    public readonly ref struct GroupsEnumerable<T1, T2, T3, T4> where T1 : struct, _IInternalEntityComponent
                                                                where T2 : struct, _IInternalEntityComponent
                                                                where T3 : struct, _IInternalEntityComponent
                                                                where T4 : struct, _IInternalEntityComponent
    {
        public GroupsEnumerable(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _db     = db;
            _groups = groups;
        }

        public ref struct GroupsIterator
        {
            public GroupsIterator(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _groups     = groups;
                _indexGroup = -1;
                _entitiesDB = db;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.count)
                {
                    var exclusiveGroupStruct = _groups[_indexGroup];
                    if (exclusiveGroupStruct.IsEnabled() == false)
                        continue;

                    var entityCollection = _entitiesDB.QueryEntities<T1, T2, T3, T4>(exclusiveGroupStruct);
                    if (entityCollection.count == 0)
                        continue;

                    _buffers = entityCollection;
                    break;
                }

                var moveNext = _indexGroup < _groups.count;

                if (moveNext == false)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexGroup = -1; }

            public RefCurrent Current => new RefCurrent(_buffers, _groups[_indexGroup]);
            public bool       isValid => _indexGroup != -1;

            readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

            int                              _indexGroup;
            EntityCollection<T1, T2, T3, T4> _buffers;
            readonly EntitiesDB              _entitiesDB;
        }

        public GroupsIterator GetEnumerator() { return new GroupsIterator(_db, _groups); }

        readonly EntitiesDB                                    _db;
        readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        public readonly ref struct RefCurrent
        {
            public RefCurrent(in EntityCollection<T1, T2, T3, T4> buffers, ExclusiveGroupStruct group)
            {
                _buffers = buffers;
                _group   = group;
            }

            public void Deconstruct(out EntityCollection<T1, T2, T3, T4> buffers, out ExclusiveGroupStruct group)
            {
                buffers = _buffers;
                group   = _group;
            }

            public readonly EntityCollection<T1, T2, T3, T4> _buffers;
            public readonly ExclusiveGroupStruct             _group;
        }
    }

    public readonly ref struct GroupsEnumerable<T1, T2, T3> where T1 : struct, _IInternalEntityComponent
                                                            where T2 : struct, _IInternalEntityComponent
                                                            where T3 : struct, _IInternalEntityComponent
    {
        public GroupsEnumerable(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _db     = db;
            _groups = groups;
        }

        public ref struct GroupsIterator
        {
            public GroupsIterator(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _groups     = groups;
                _indexGroup = -1;
                _entitiesDB = db;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.count)
                {
                    var exclusiveGroupStruct = _groups[_indexGroup];
                    if (exclusiveGroupStruct.IsEnabled() == false)
                        continue;

                    EntityCollection<T1, T2, T3> entityCollection = _entitiesDB.QueryEntities<T1, T2, T3>(exclusiveGroupStruct);
                    if (entityCollection.count == 0)
                        continue;

                    _buffers = entityCollection;
                    break;
                }

                var moveNext = _indexGroup < _groups.count;

                if (moveNext == false)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexGroup = -1; }

            public RefCurrent Current => new RefCurrent(_buffers, _groups[_indexGroup]);
            public bool       isValid => _indexGroup != -1;

            readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

            int                          _indexGroup;
            EntityCollection<T1, T2, T3> _buffers;
            readonly EntitiesDB          _entitiesDB;
        }

        //todo: there is a steep cost to pay to copy these structs, one day I may need to investigate this more.
        //the structs are quite big, in the order of 100+ bytes.
        public GroupsIterator GetEnumerator() { return new GroupsIterator(_db, _groups); }

        readonly EntitiesDB                                    _db;
        readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        public readonly ref struct RefCurrent
        {
            public RefCurrent(in EntityCollection<T1, T2, T3> buffers, ExclusiveGroupStruct group)
            {
                _buffers = buffers;
                _group   = group;
            }

            public void Deconstruct(out EntityCollection<T1, T2, T3> buffers, out ExclusiveGroupStruct group)
            {
                buffers = _buffers;
                group   = _group;
            }

            public readonly EntityCollection<T1, T2, T3> _buffers;
            public readonly ExclusiveGroupStruct         _group;
        }
    }

    public readonly ref struct GroupsEnumerable<T1, T2>
        where T1 : struct, _IInternalEntityComponent where T2 : struct, _IInternalEntityComponent
    {
        public GroupsEnumerable(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _db     = db;
            _groups = groups;
        }

        public ref struct GroupsIterator
        {
            public GroupsIterator(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _db         = db;
                _groups     = groups;
                _indexGroup = -1;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.count)
                {
                    var exclusiveGroupStruct = _groups[_indexGroup];
                    if (exclusiveGroupStruct.IsEnabled() == false)
                        continue;

                    var entityCollection = _db.QueryEntities<T1, T2>(exclusiveGroupStruct);
                    if (entityCollection.count == 0)
                        continue;

                    _buffers = entityCollection;
                    break;
                }

                var moveNext = _indexGroup < _groups.count;

                if (moveNext == false)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexGroup = -1; }

            public RefCurrent Current => new RefCurrent(_buffers, _groups[_indexGroup]);
            public bool       isValid => _indexGroup != -1;

            readonly EntitiesDB                                    _db;
            readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

            int                      _indexGroup;
            EntityCollection<T1, T2> _buffers;
        }

        public GroupsIterator GetEnumerator() { return new GroupsIterator(_db, _groups); }

        readonly EntitiesDB                                    _db;
        readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        public readonly ref struct RefCurrent
        {
            public RefCurrent(in EntityCollection<T1, T2> buffers, ExclusiveGroupStruct group)
            {
                _buffers = buffers;
                _group   = group;
            }

            public void Deconstruct(out EntityCollection<T1, T2> buffers, out ExclusiveGroupStruct group)
            {
                buffers = _buffers;
                group   = _group;
            }

            public readonly EntityCollection<T1, T2> _buffers;
            public readonly ExclusiveGroupStruct     _group;
        }
    }

    public readonly ref struct GroupsEnumerable<T1> where T1 : struct, _IInternalEntityComponent
    {
        public GroupsEnumerable(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _db     = db;
            _groups = groups;
        }

        public ref struct GroupsIterator
        {
            public GroupsIterator(EntitiesDB db, in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _db         = db;
                _groups     = groups;
                _indexGroup = -1;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.count)
                {
                    var exclusiveGroupStruct = _groups[_indexGroup];
                    
                    if (exclusiveGroupStruct.IsEnabled() == false)
                        continue;

                    var entityCollection = _db.QueryEntities<T1>(exclusiveGroupStruct);
                    if (entityCollection.count == 0)
                        continue;

                    _buffer = entityCollection;
                    break;
                }

                var moveNext = _indexGroup < _groups.count;

                if (moveNext == false)
                    Reset();

                return moveNext;
            }

            public void Reset() { _indexGroup = -1; }

            public RefCurrent Current => new RefCurrent(_buffer, _groups[_indexGroup]);
            public bool       isValid => _indexGroup != -1;

            readonly EntitiesDB                                    _db;
            readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

            int                  _indexGroup;
            EntityCollection<T1> _buffer;
        }

        public GroupsIterator GetEnumerator() { return new GroupsIterator(_db, _groups); }

        readonly EntitiesDB                                    _db;
        readonly LocalFasterReadOnlyList<ExclusiveGroupStruct> _groups;

        public readonly ref struct RefCurrent
        {
            public RefCurrent(in EntityCollection<T1> buffers, in ExclusiveGroupStruct group)
            {
                _buffers = buffers;
                _group   = group;
            }

            public void Deconstruct(out EntityCollection<T1> buffers, out ExclusiveGroupStruct group)
            {
                buffers = _buffers;
                group   = _group;
            }

            public void Deconstruct(out EntityCollection<T1> buffers)
            {
                buffers = _buffers;
            }

            internal readonly EntityCollection<T1> _buffers;
            internal readonly ExclusiveGroupStruct _group;
        }
    }
}