using System;
using System.Collections.Generic;

namespace Svelto.ECS.Schedulers
{
    public sealed class SimpleEntitiesSubmissionScheduler : EntitiesSubmissionScheduler
    {
        public SimpleEntitiesSubmissionScheduler(uint maxNumberOfOperationsPerFrame = UInt32.MaxValue)
        {
            _enumerator = SubmitEntitiesAsync(maxNumberOfOperationsPerFrame);
        }

        public IEnumerator<bool> SubmitEntitiesAsync() { return _enumerator; }

        public IEnumerator<bool> SubmitEntitiesAsync(uint maxNumberOfOperations)
        {
            EnginesRoot.EntitiesSubmitter entitiesSubmitter = _onTick.Value;
            entitiesSubmitter.maxNumberOfOperationsPerFrame = maxNumberOfOperations;

            while (true)
            {
                if (paused == false)
                {
                    var entitiesSubmitterSubmitEntities = entitiesSubmitter.submitEntities;

                    entitiesSubmitterSubmitEntities.MoveNext();

                    if (entitiesSubmitterSubmitEntities.Current == true)
                        yield return true;
                    else
                        yield return false;
                }
            }
        }

        public void SubmitEntities()
        {
            _enumerator.MoveNext();

            while (_enumerator.Current == true)
                _enumerator.MoveNext();
        }

        public override bool paused    { get; set; }
        public override void Dispose() { }

        protected internal override EnginesRoot.EntitiesSubmitter onTick
        {
            set
            {
                DBC.ECS.Check.Require(_onTick == null, "a scheduler can be exclusively used by one enginesRoot only");

                _onTick = value;
            }
        }

        EnginesRoot.EntitiesSubmitter? _onTick;
        readonly IEnumerator<bool>     _enumerator;
    }
}