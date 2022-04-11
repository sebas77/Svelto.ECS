#if UNITY_ECS
using System;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.SveltoOnDOTS
{
    /// <summary>
    /// SubmissionEngine is a dedicated DOTS ECS Svelto.ECS engine that allows using the DOTS ECS
    /// EntityCommandBuffer for fast creation of DOTS entities
    /// </summary>
    public abstract class SveltoOnDOTSHandleCreationEngine
    {
        protected EntityCommandBufferForSvelto ECB { get; private set; }

        protected internal EntityManager entityManager
        {
            // [Obsolete(
            //     "<color=orange>Attention: the use of EntityManager directly is deprecated. ECB MUST BE USED INSTEAD</color>")]
            get;
            internal set;
        }

        internal EntityCommandBufferForSvelto entityCommandBuffer
        {
            set => ECB = value;
        }

        protected EntityArchetype CreateArchetype(params ComponentType[] types)
        {
            return entityManager.CreateArchetype(types);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Entity CreateDOTSEntityOnSvelto(Entity entityComponentPrefabEntity, EGID egid,
            bool mustHandleDOTSComponent)
        {
            return ECB.CreateDOTSEntityOnSvelto(entityComponentPrefabEntity, egid, mustHandleDOTSComponent);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Entity CreateDOTSEntityOnSvelto(EntityArchetype archetype, EGID egid, bool mustHandleDOTSComponent)
        {
            return ECB.CreateDOTSEntityOnSvelto(archetype, egid, mustHandleDOTSComponent);
        }
        
        protected internal virtual void OnCreate()
        {
        }

        protected internal virtual JobHandle OnUpdate()
        {
            return default;
        }

        public abstract string name { get; }
    }
}
#endif