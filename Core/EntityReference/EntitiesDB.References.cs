using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;

namespace Svelto.ECS
{
    public partial class EntitiesDB
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetEGID(EntityReference entityReference, out EGID egid)
        {
            return _entityReferencesMap.TryGetEGID(entityReference, out egid);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EGID GetEGID(EntityReference entityReference)
        {
            return _entityReferencesMap.GetEGID(entityReference);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EnginesRoot.EntityReferenceMap GetEntityReferenceMap()
        {
            return _entityReferencesMap;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityReference GetEntityReference(EGID egid)
        {
            return _entityReferencesMap.GetEntityReference(egid);
        }

        public SharedSveltoDictionaryNative<uint, EntityReference> GetEntityReferenceMap(ExclusiveGroupStruct groupID)
        {
            return _entityReferencesMap.GetEntityReferenceMap(groupID);
        }
    }
}