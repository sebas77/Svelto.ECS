//This profiler is based on the Entitas Visual Debugging tool 
//https://github.com/sschmid/Entitas-CSharp

namespace Svelto.ECS.Profiler
{
    public sealed class EngineInfo
    {
        readonly IEngine _engine;
        readonly string _engineName;

        const int NUM_UPDATE_TYPES = 3;
        const int NUM_FRAMES_TO_AVERAGE = 10;

        double _accumulatedAddDuration;
        double _minAddDuration;
        double _maxAddDuration;
        int _entityViewsAddedCount;

        double _accumulatedRemoveDuration;
        double _minRemoveDuration;
        double _maxRemoveDuration;
        int _entityViewsRemovedCount;

        public string engineName { get { return _engineName; } }
        
        public double minAddDuration { get { return _minAddDuration; } }
        public double minRemoveDuration { get { return _minRemoveDuration; } }

        public double maxAddDuration { get { return _maxAddDuration; } }
        public double maxRemoveDuration { get { return _maxRemoveDuration; } }

        public double averageAddDuration { get { return _entityViewsAddedCount == 0 ? 0 : _accumulatedAddDuration / _entityViewsAddedCount; } }
        public double averageRemoveDuration { get { return _entityViewsRemovedCount == 0 ? 0 : _accumulatedRemoveDuration / _entityViewsRemovedCount; } }

        public EngineInfo(IEngine engine)
        {
            _engine = engine;
            _engineName = _engine.ToString();

            int foundNamespace = _engineName.LastIndexOf(".");
            _engineName = _engineName.Remove(0, foundNamespace + 1);
            
            ResetDurations();
        }

        public void AddAddDuration(double duration)
        {
            if (duration < _minAddDuration || _minAddDuration == 0)
            {
                _minAddDuration = duration;
            }
            if (duration > _maxAddDuration)
            {
                _maxAddDuration = duration;
            }
            _accumulatedAddDuration += duration;
            _entityViewsAddedCount += 1;
        }

        public void AddRemoveDuration(double duration)
        {
            if (duration < _minRemoveDuration || _minRemoveDuration == 0)
            {
                _minRemoveDuration = duration;
            }
            if (duration > _maxRemoveDuration)
            {
                _maxRemoveDuration = duration;
            }
            _accumulatedRemoveDuration += duration;
            _entityViewsRemovedCount += 1;
        }

        public void ResetDurations()
        {
            _accumulatedAddDuration = 0;
            _minAddDuration = 0;
            _maxAddDuration = 0;
            _entityViewsAddedCount = 0;

            _accumulatedRemoveDuration = 0;
            _minRemoveDuration = 0;
            _maxRemoveDuration = 0;
            _entityViewsRemovedCount = 0;
        }
    }
}
