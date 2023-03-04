using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public readonly ref struct NativeEntityFilterIterator<T> where T : unmanaged, _IInternalEntityComponent
    {
        internal NativeEntityFilterIterator(NativeEntityFilterCollection<T> filter)
        {
            _filter = filter;
        }

        public Iterator GetEnumerator() => new Iterator(_filter);

        readonly NativeEntityFilterCollection<T> _filter;

        public ref struct Iterator
        {
            internal Iterator(NativeEntityFilterCollection<T>  filter)
            {
                _filter     = filter;
                _indexGroup = -1;
                _current    = default;
            }

            public bool MoveNext()
            {
                while (++_indexGroup < _filter.groupCount)
                {
                    _current = _filter.GetGroup(_indexGroup);

                    if (_current.count > 0) break;
                }

                return _indexGroup < _filter.groupCount;
            }

            public void Reset()
            {
                _indexGroup = -1;
            }

            public RefCurrent Current => new RefCurrent(_current);

            int                                          _indexGroup;
            readonly NativeEntityFilterCollection<T>     _filter;
            NativeEntityFilterCollection<T>.GroupFilters _current;
        }

        public readonly ref struct RefCurrent
        {
            internal RefCurrent(NativeEntityFilterCollection<T>.GroupFilters filter)
            {
                _filter = filter;
            }

            public void Deconstruct(out EntityFilterIndices indices, out ExclusiveGroupStruct group)
            {
                indices = _filter.indices;
                group   = _filter.group;
            }

            readonly NativeEntityFilterCollection<T>.GroupFilters _filter;
        }
    }
}