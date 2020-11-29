using System;
using System.Collections;

namespace Svelto.ECS
{
    public class WaitForSubmissionEnumerator : IEnumerator
    {
        class SubmissionEntityDescriptor : GenericEntityDescriptor<SubmissionSignalStruct>
        {
            internal static readonly ExclusiveGroup SubmissionGroup = new ExclusiveGroup();
        }

        readonly IEntityFactory   _entityFactory;
        readonly EntitiesDB      _entitiesDB;
        readonly IEntityFunctions _entityFunctions;

        int                       _state;

        public WaitForSubmissionEnumerator(IEntityFunctions entityFunctions, IEntityFactory entityFactory,
            EntitiesDB entitiesDb)
        {
            _entityFactory = entityFactory;
            _entityFunctions = entityFunctions;
            _entitiesDB = entitiesDb;
        }
        
        public bool MoveNext()
        {
            switch (_state)
            {
                case 0:
                    _counter = _COUNTER++;
                    _entityFactory.BuildEntity<SubmissionEntityDescriptor>(new EGID((uint) _counter,
                        SubmissionEntityDescriptor.SubmissionGroup));
                    _state = 1;
                    return true;
                case 1:
                    if (_entitiesDB.Exists<SubmissionSignalStruct>(new EGID((uint) _counter,
                            SubmissionEntityDescriptor.SubmissionGroup)) == false)
                        return true;

                    _entityFunctions.RemoveEntity<SubmissionEntityDescriptor>(new EGID((uint) _counter,
                        SubmissionEntityDescriptor.SubmissionGroup));
                    _state = 0;
                    return false;
            }

            throw new Exception("something is wrong");
        }

        void IEnumerator.Reset()
        {
            throw new NotImplementedException();
        }

        public object Current { get; }

        struct SubmissionSignalStruct : IEntityComponent
        {}

        int        _counter;
        static int _COUNTER;
    }
}    