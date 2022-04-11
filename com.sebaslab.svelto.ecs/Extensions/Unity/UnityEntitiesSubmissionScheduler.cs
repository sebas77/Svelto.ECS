#if UNITY_5 || UNITY_5_3_OR_NEWER
using System;
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

        void SubmitEntities()
        {
            try
            {
                _onTick.SubmitEntities();
            }
            catch (Exception e)
            {
                paused = true;
                
                Svelto.Console.LogException(e);
                
                throw;
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