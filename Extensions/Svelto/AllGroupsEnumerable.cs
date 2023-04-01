using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    /// <summary>
    /// ToDo it would be interesting to have a version of this dedicated to unmanaged, IEntityComponent
    /// that can be burstifiable
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public readonly ref struct AllGroupsEnumerable<T1> where T1 : struct, _IInternalEntityComponent
    {
        public readonly ref struct GroupCollection
        {
            readonly EntityCollection<T1> collection;
            readonly ExclusiveGroupStruct group;

            public GroupCollection(EntityCollection<T1> entityCollection, ExclusiveGroupStruct groupKey)
            {
                collection = entityCollection;
                group = groupKey;
            }

            public void Deconstruct(out EntityCollection<T1> collection, out ExclusiveGroupStruct group)
            {
                collection = this.collection;
                group      = this.@group;
            }
        }

        public AllGroupsEnumerable(EntitiesDB db) { _db = db; }

        public ref struct GroupsIterator
        {
            public GroupsIterator(EntitiesDB db) : this()
            {
                _db = db.FindGroups_INTERNAL(ComponentTypeID<T1>.id).GetEnumerator();
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (_db.MoveNext() == true)
                {
                    var group = _db.Current;
                    if (group.key.IsEnabled() == false)
                        continue;

                    ITypeSafeDictionary<T1> typeSafeDictionary = @group.value as ITypeSafeDictionary<T1>;

                    if (typeSafeDictionary.count == 0)
                        continue;

                    _array = new GroupCollection(
                        new EntityCollection<T1>(
                            typeSafeDictionary.GetValues(out var count),
                            typeSafeDictionary.entityIDs, count), group.key);
                    return true;
                }

                return false;
            }

            public GroupCollection Current => _array;

            SveltoDictionaryKeyValueEnumerator<ExclusiveGroupStruct, ITypeSafeDictionary,
                ManagedStrategy<SveltoDictionaryNode<ExclusiveGroupStruct>>, ManagedStrategy<ITypeSafeDictionary>,
                ManagedStrategy<int>> _db;

            GroupCollection _array;
        }

        public GroupsIterator GetEnumerator() { return new GroupsIterator(_db); }

        readonly EntitiesDB _db;
    }
}