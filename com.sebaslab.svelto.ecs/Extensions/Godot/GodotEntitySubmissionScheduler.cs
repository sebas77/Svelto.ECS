#if GODOT
using System;
using Godot;

namespace Svelto.ECS.Schedulers.Godot
{
    //The EntitySubmissionScheduler has been introduced to make the entity components submission logic platform independent
    //You can customize the scheduler if you wish
    public class GodotEntitySubmissionScheduler : EntitiesSubmissionScheduler
    {
        readonly GodotScheduler        _scheduler;
        EnginesRoot.EntitiesSubmitter _onTick;

        /// <summary>
        /// Unlike unity. Creating a gameObject with new GameObject(name) will not add it to the current scene
        /// It needs to be added as a child to the scene (which is also a ~~gameObject~~Node)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        public GodotEntitySubmissionScheduler(string name, Node parent)
        {
            
            _scheduler = new GodotScheduler();
            _scheduler.onTick = SubmitEntities;
            parent.AddChild(_scheduler);
        }

        private void SubmitEntities()
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

        public override void Dispose()
        {
            if (_scheduler != null)
            {
                _scheduler.QueueFree();
            }
        }
    }

}
#endif