#if UNITY_5 || UNITY_5_3_OR_NEWER
using Object = UnityEngine.Object;
using UnityEngine;

namespace Svelto.ECS.Schedulers.Unity
{
    //The EntitySubmissionScheduler has been introduced to make the entity components submission logic platform independent
    //You can customize the scheduler if you wish
    public class UnityEntitiesSubmissionScheduler : EntitiesSubmissionScheduler
    {
        public UnityEntitiesSubmissionScheduler(string name)
        {
            _scheduler = new GameObject(name).AddComponent<MonoScheduler>();
            GameObject.DontDestroyOnLoad(_scheduler.gameObject);
            _scheduler.onTick = SubmitEntities;
        }

        public override void Dispose()
        {
            if (_scheduler != null && _scheduler.gameObject != null)
            {
                Object.Destroy(_scheduler.gameObject);
            }
        }

        public override bool paused { get; set; }

        void SubmitEntities()
        {
            if (paused == false)
            {
                var enumerator = _onTick.submitEntities;
                enumerator.MoveNext();
                    
                while (enumerator.Current == true) enumerator.MoveNext();
            }
        }

        protected internal override EnginesRoot.EntitiesSubmitter onTick
        {
            set => _onTick = value;
        }

        readonly MonoScheduler        _scheduler;
        EnginesRoot.EntitiesSubmitter _onTick;
    }
}
#endif