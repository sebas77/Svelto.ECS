using System;
using System.Collections.Generic;

//This profiler is based on the Entitas Visual Debugging tool 
//https://github.com/sschmid/Entitas-CSharp

namespace Svelto.ECS.Profiler
{
    public sealed class EngineInfo
    {
        enum UpdateType
        {
            Update = 0,
            LateUpdate = 1,
            FixedUpdate = 2,
        }

        readonly IEngine _engine;
        readonly string _engineName;
        readonly Type _engineType;

        const int NUM_UPDATE_TYPES = 3;
        const int NUM_FRAMES_TO_AVERAGE = 10;

        //use a queue to averave out the last 30 frames
        Queue<double>[] _updateFrameTimes = new Queue<double>[NUM_UPDATE_TYPES];
        readonly double[] _accumulatedUpdateDuration = new double[NUM_UPDATE_TYPES];
        readonly double[] _lastUpdateDuration = new double[NUM_UPDATE_TYPES];

        readonly double[] _minUpdateDuration = new double[NUM_UPDATE_TYPES];
        readonly double[] _maxUpdateDuration = new double[NUM_UPDATE_TYPES];

        double _accumulatedAddDuration;
        double _minAddDuration;
        double _maxAddDuration;
        int _entityViewsAddedCount;

        double _accumulatedRemoveDuration;
        double _minRemoveDuration;
        double _maxRemoveDuration;
        int _entityViewsRemovedCount;

        public IEngine engine { get { return _engine; } }
        public string engineName { get { return _engineName; } }
        public Type engineType { get { return _engineType; } }

        public double lastUpdateDuration { get { return _lastUpdateDuration[(int) UpdateType.Update]; } }
        public double lastFixedUpdateDuration { get { return _lastUpdateDuration[(int)UpdateType.LateUpdate]; } }
        public double lastLateUpdateDuration { get { return _lastUpdateDuration[(int)UpdateType.FixedUpdate]; } }

        public double minAddDuration { get { return _minAddDuration; } }
        public double minRemoveDuration { get { return _minRemoveDuration; } }
        public double minUpdateDuration { get { return _minUpdateDuration[(int)UpdateType.Update]; } }

        public double maxAddDuration { get { return _maxAddDuration; } }
        public double maxRemoveDuration { get { return _maxRemoveDuration; } }
        public double maxUpdateDuration { get { return _maxUpdateDuration[(int)UpdateType.Update]; } }

        public double averageAddDuration { get { return _entityViewsAddedCount == 0 ? 0 : _accumulatedAddDuration / _entityViewsAddedCount; } }
        public double averageRemoveDuration { get { return _entityViewsRemovedCount == 0 ? 0 : _accumulatedRemoveDuration / _entityViewsRemovedCount; } }
        public double averageUpdateDuration { get { return _updateFrameTimes[(int)UpdateType.Update].Count == 0 ? 0 : _accumulatedUpdateDuration[(int)UpdateType.Update] / _updateFrameTimes[(int)UpdateType.Update].Count; } }
        public double averageLateUpdateDuration { get { return _updateFrameTimes[(int)UpdateType.LateUpdate].Count == 0 ? 0 : _accumulatedUpdateDuration[(int)UpdateType.LateUpdate] / _updateFrameTimes[(int)UpdateType.LateUpdate].Count; } }
        public double averageFixedUpdateDuration { get { return _updateFrameTimes[(int)UpdateType.FixedUpdate].Count == 0 ? 0 : _accumulatedUpdateDuration[(int)UpdateType.FixedUpdate] / _updateFrameTimes[(int)UpdateType.FixedUpdate].Count; } }

        public EngineInfo(IEngine engine)
        {
            _engine = engine;
            _engineName = _engine.ToString();

            int foundNamespace = _engineName.LastIndexOf(".");
            _engineName = _engineName.Remove(0, foundNamespace + 1);

            _engineType = engine.GetType();

            for (int i = 0; i < NUM_UPDATE_TYPES; i++)
            {
                _updateFrameTimes[i] = new Queue<double>();
            }
            ResetDurations();
        }

        public void AddUpdateDuration(double updateDuration)
        {
            AddUpdateDurationForType(updateDuration, (int)UpdateType.Update);
        }

        public void AddLateUpdateDuration(double updateDuration)
        {
            AddUpdateDurationForType(updateDuration, (int)UpdateType.LateUpdate);
        }

        public void AddFixedUpdateDuration(double updateDuration)
        {
            AddUpdateDurationForType(updateDuration, (int)UpdateType.FixedUpdate);
        }

        void AddUpdateDurationForType(double updateDuration, int updateType)
        {
            if (updateDuration < _minUpdateDuration[updateType] || _minUpdateDuration[updateType] == 0)
            {
                _minUpdateDuration[updateType] = updateDuration;
            }
            if (updateDuration > _maxUpdateDuration[updateType])
            {
                _maxUpdateDuration[updateType] = updateDuration;
            }

            if (_updateFrameTimes[updateType].Count == NUM_FRAMES_TO_AVERAGE)
            {
                _accumulatedUpdateDuration[updateType] -= _updateFrameTimes[updateType].Dequeue();
            }

            _accumulatedUpdateDuration[updateType] += updateDuration;
            _updateFrameTimes[updateType].Enqueue(updateDuration);
            _lastUpdateDuration[updateType] = updateDuration;
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
            for (int i = 0; i < NUM_UPDATE_TYPES; i++)
            {
                _accumulatedUpdateDuration[i] = 0;
                _minUpdateDuration[i] = 0;
                _maxUpdateDuration[i] = 0;
                _updateFrameTimes[i].Clear();
            }

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
