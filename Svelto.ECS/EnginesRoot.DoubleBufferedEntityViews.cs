﻿using Svelto.DataStructures.Experimental;
using EntitiesDB =
    Svelto.DataStructures.Experimental.FasterDictionary<uint, System.Collections.Generic.Dictionary<System.Type,
        Svelto.ECS.Internal.ITypeSafeDictionary>>;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        internal class DoubleBufferedEntitiesToAdd
        {
            internal void Swap()
            {
                Swap(ref current, ref other);
                Swap(ref currentEntitiesCreatedPerGroup, ref otherEntitiesCreatedPerGroup);
            }

            void Swap<T>(ref T item1, ref T item2)
            {
                T toSwap = item2; item2 = item1; item1 = toSwap;
            }

            public void ClearOther()
            {
                //do not clear the groups created so far, they will be reused
                foreach (var groups in other)
                {
                    //do not remove the dictionaries of entities per type created so far, they will be reused
                    foreach (var entitiesPerType in groups.Value)
                    {
                       //clear the dictionary of entities create do far (it won't allocate though)
                        entitiesPerType.Value.Clear();
                    }
                }

                otherEntitiesCreatedPerGroup.Clear();
            }
            
            internal FasterDictionary<uint, uint> currentEntitiesCreatedPerGroup;
            internal FasterDictionary<uint, uint> otherEntitiesCreatedPerGroup;
            
            internal EntitiesDB current;
            internal EntitiesDB other;

            readonly EntitiesDB _entityViewsToAddBufferA = new EntitiesDB();
            readonly EntitiesDB _entityViewsToAddBufferB = new EntitiesDB();

            readonly FasterDictionary<uint, uint> _entitiesCreatedPerGroupA = new FasterDictionary<uint, uint>();
            readonly FasterDictionary<uint, uint> _entitiesCreatedPerGroupB = new FasterDictionary<uint, uint>();
            
            public DoubleBufferedEntitiesToAdd()
            {
                currentEntitiesCreatedPerGroup = _entitiesCreatedPerGroupA;
                otherEntitiesCreatedPerGroup = _entitiesCreatedPerGroupB;
                
                current = _entityViewsToAddBufferA;
                other = _entityViewsToAddBufferB;
            }
        }
    }
}