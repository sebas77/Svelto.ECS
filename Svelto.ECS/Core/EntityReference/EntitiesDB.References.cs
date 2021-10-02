using System.Runtime.CompilerServices;
using Svelto.ECS.Reference;

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
        public EnginesRoot.LocatorMap GetEntityLocator()
        {
            return _entityReferencesMap;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityReference GetEntityReference(EGID egid)
        {
            return _entityReferencesMap.GetEntityReference(egid);
        }
    }
}