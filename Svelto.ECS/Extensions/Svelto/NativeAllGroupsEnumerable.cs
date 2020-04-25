using System;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public readonly struct NativeAllGroupsEnumerable<T1> where T1 : unmanaged, IEntityComponent
    {
        public NativeAllGroupsEnumerable(EntitiesDB db)
        {
            _db = db;
        }
        
        public struct NativeGroupsIterator
        {
            public struct CurrentGroup: IDisposable
            {
                public NB<T1> buffer;
                public ExclusiveGroupStruct group;

                public void Dispose()
                {
                    buffer.Dispose();
                }
            }
            
            public NativeGroupsIterator(EntitiesDB db) : this()
            {
                _db = db.FindGroups<T1>().GetEnumerator();
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (_db.MoveNext() == true)
                {
                    FasterDictionary<uint, ITypeSafeDictionary>.KeyValuePairFast group = _db.Current;

                    ITypeSafeDictionary<T1> typeSafeDictionary = @group.Value as ITypeSafeDictionary<T1>;
                    
                    if (typeSafeDictionary.Count == 0) continue;
                    
                    _array.buffer = new EntityCollection<T1>(typeSafeDictionary.GetValuesArray(out var count), count)
                        .ToNativeBuffer<T1>();
                    _array.@group = new ExclusiveGroupStruct(group.Key);

                    return true;
                }

                return false;
            }

            public void Reset()
            {
            }

            public CurrentGroup Current => _array;

            FasterDictionary<uint, ITypeSafeDictionary>.FasterDictionaryKeyValueEnumerator _db;
            CurrentGroup _array;
        }

        public NativeGroupsIterator GetEnumerator()
        {
            return new NativeGroupsIterator(_db);
        }

        readonly EntitiesDB       _db;
    }
#if TO_BE_FINISHED
    public struct NativeAllGroupsEnumerable<T1, T2> 
        where T1 : unmanaged, IEntityComponent where T2 : unmanaged, IEntityComponent
    {
        public NativeAllGroupsEnumerable(EntitiesDB db)
        {
            _db = db;
        }

        public struct NativeGroupsIterator
        {
            public NativeGroupsIterator(EntitiesDB db) : this()
            {
                _db = db.FindGroups<T1, T2>().GetEnumerator();
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (_db.MoveNext() == true)
                {
                    FasterDictionary<uint, ITypeSafeDictionary>.KeyValuePairFast group = _db.Current;

                    ITypeSafeDictionary<T1> typeSafeDictionary1 = @group.Value as ITypeSafeDictionary<T1>;
                    ITypeSafeDictionary<T2> typeSafeDictionary2 = @group.Value as ITypeSafeDictionary<T2>;

                    DBC.ECS.Check.Require(typeSafeDictionary1.Count != typeSafeDictionary2.Count
                                        , "entities count do not match"); 
                        
                    if (typeSafeDictionary1.Count == 0) continue;
                    
                    _array = new BT<NB<T1>, NB<T2>>()(new EntityCollection<T1>(typeSafeDictionary1.GetValuesArray(out var count), count)
                       .ToNativeBuffer<T1>();

                    return true;
                }

                return false;
            }

            public void Reset()
            {
            }

            public BT<NB<T1>, NB<T2>> Current => _array;

            FasterDictionary<uint, ITypeSafeDictionary>.FasterDictionaryKeyValueEnumerator _db;

            BT<NB<T1>, NB<T2>> _array;
        }

        public NativeGroupsIterator GetEnumerator()
        {
            return new NativeGroupsIterator(_db);
        }

        readonly EntitiesDB _db;
    }
#endif
}
