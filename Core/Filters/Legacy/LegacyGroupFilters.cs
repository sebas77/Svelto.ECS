#if SVELTO_LEGACY_FILTERS
using Svelto.DataStructures;
using Svelto.DataStructures.Native;

namespace Svelto.ECS
{
    public struct LegacyGroupFilters
    {
        internal LegacyGroupFilters(SharedSveltoDictionaryNative<int, LegacyFilterGroup> legacyFilters, ExclusiveGroupStruct group)
        {
            this._legacyFilters = legacyFilters;
            _group       = @group;
        }

        public ref LegacyFilterGroup GetFilter(int filterIndex)
        {
#if DEBUG && !PROFILE_SVELTO
            if (_legacyFilters.isValid == false)
                throw new ECSException($"trying to fetch not existing filters {filterIndex} group {_group.ToName()}");
            if (_legacyFilters.ContainsKey(filterIndex) == false)
                throw new ECSException($"trying to fetch not existing filters {filterIndex} group {_group.ToName()}");
#endif
            return ref _legacyFilters.GetValueByRef(filterIndex);
        }

        public bool HasFilter(int filterIndex) { return _legacyFilters.ContainsKey(filterIndex); }

        public void ClearFilter(int filterIndex)
        {
            if (_legacyFilters.TryFindIndex(filterIndex, out var index))
                _legacyFilters.GetValues(out _)[index].Clear();
        }

        public void ClearFilters()
        {
            foreach (var filter in _legacyFilters)
                filter.value.Clear();
        }

        public bool TryGetFilter(int filterIndex, out LegacyFilterGroup legacyFilter)
        {
            return _legacyFilters.TryGetValue(filterIndex, out legacyFilter);
        }

        public SveltoDictionaryKeyValueEnumerator<int, LegacyFilterGroup, NativeStrategy<SveltoDictionaryNode<int>>, NativeStrategy<LegacyFilterGroup>
          , NativeStrategy<int>> GetEnumerator()
        {
            return _legacyFilters.GetEnumerator();
        }
        
        //Note the following methods are internal because I was pondering the idea to be able to return
        //the list of LegacyGroupFilters linked to a specific filter ID. However this would mean to be able to
        //maintain a revers map which at this moment seems too much and also would need the following
        //method to be for ever internal (at this point in time I am not sure it's a good idea)
        internal void DisposeFilter(int filterIndex)
        {
            if (_legacyFilters.TryFindIndex(filterIndex, out var index))
            {
                ref var filterGroup = ref _legacyFilters.GetValues(out _)[index];
                
                filterGroup.Dispose();

                _legacyFilters.Remove(filterIndex);
            }
        }

        internal void DisposeFilters()
        {
            //must release the native buffers!
            foreach (var filter in _legacyFilters)
                filter.value.Dispose();

            _legacyFilters.Clear();
        }

        internal ref LegacyFilterGroup CreateOrGetFilter(int filterID)
        {
            if (_legacyFilters.TryFindIndex(filterID, out var index) == false)
            {
                var orGetFilterForGroup = new LegacyFilterGroup(_group, filterID);

                _legacyFilters[filterID] = orGetFilterForGroup;

                return ref _legacyFilters.GetValueByRef(filterID);
            }

            return ref _legacyFilters.GetValues(out _)[index];
        }

        internal void Dispose()
        {
            foreach (var filter in _legacyFilters)
            {
                filter.value.Dispose();
            }

            _legacyFilters.Dispose();
        }

        readonly ExclusiveGroupStruct _group;

        //filterID, filter
        SharedSveltoDictionaryNative<int, LegacyFilterGroup> _legacyFilters;
    }
}
#endif