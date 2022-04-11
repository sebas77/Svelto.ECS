using System;
using System.Collections;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS
{
    /// <summary>
    /// Enumerator that yields until the next Entities Submission
    /// </summary>
    public class WaitForSubmissionEnumerator : IEnumerator
    {
        public WaitForSubmissionEnumerator(EntitiesSubmissionScheduler scheduler)
        {
            _scheduler       = scheduler;
        }
        
        public bool MoveNext()
        {
            switch (_state)
            {
                case 0:
                    _iteration = _scheduler.iteration;
                    _state     = 1;
                    return true; 
                case 1:
                    if (_iteration != _scheduler.iteration)
                    {
                        _state = 0;
                        return false;
                    }
                    return true;
            }

            throw new Exception("something is wrong");
        }

        void IEnumerator.Reset()
        {
            throw new NotImplementedException();
        }

        public object Current { get; }

        readonly EntitiesSubmissionScheduler _scheduler;
        uint                                 _state;
        uint                                 _iteration;
    }
}    