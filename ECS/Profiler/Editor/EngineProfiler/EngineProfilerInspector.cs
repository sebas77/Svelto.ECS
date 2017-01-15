using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//This profiler is based on the Entitas Visual Debugging tool 
//https://github.com/sschmid/Entitas-CSharp

namespace Svelto.ECS.Profiler
{
    [CustomEditor(typeof (EngineProfilerBehaviour))]
    public class EngineProfilerInspector : Editor
    {
        enum SORTING_OPTIONS
        {
            AVERAGE,
            MIN,
            MAX,
            NAME,
            NONE
        }

        static bool _hideEmptyEngines = true;
        static bool _showTickEngines;
        static bool _showAddEngines;
        static bool _showRemoveEngines;

        static string _systemNameSearchTerm = string.Empty;

        float _axisUpperBounds = 2f;

        string updateTitle = "Update".PadRight(15, ' ');
        string lateUpdateTitle = "Late".PadRight(13, ' ');
        string fixedupdateTitle = "Fixed".PadRight(15, ' ');
        string minTitle = "Min".PadRight(15, ' ');
        string maxTitle = "Max".PadRight(15, ' ');
        string avgTitle = "Avg".PadRight(15, ' ');

        EnginesMonitor _enginesMonitor;
        Queue<float> _engineMonitorData;
        const int SYSTEM_MONITOR_DATA_LENGTH = 300;
        SORTING_OPTIONS _sortingOption = SORTING_OPTIONS.AVERAGE;

        public override void OnInspectorGUI()
        {
            var engineProfilerBehaviour = (EngineProfilerBehaviour) target;
            EngineInfo[] engines = new EngineInfo[engineProfilerBehaviour.engines.Count];
            engineProfilerBehaviour.engines.CopyTo(engines, 0);

            DrawEnginesMonitor(engines);
            DrawEngineList(engineProfilerBehaviour, engines);
            EditorUtility.SetDirty(target);
        }

        void DrawEngineList(EngineProfilerBehaviour engineProfilerBehaviour, EngineInfo[] engines)
        {
            ProfilerEditorLayout.BeginVerticalBox();
            {
                ProfilerEditorLayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Reset Durations", GUILayout.Width(120), GUILayout.Height(14)))
                    {
                        engineProfilerBehaviour.ResetDurations();
                    }
                }
                ProfilerEditorLayout.EndHorizontal();

                _sortingOption = (SORTING_OPTIONS) EditorGUILayout.EnumPopup("Sort By:", _sortingOption);

                _hideEmptyEngines = EditorGUILayout.Toggle("Hide empty systems", _hideEmptyEngines);
                EditorGUILayout.Space();

                ProfilerEditorLayout.BeginHorizontal();
                {
                    _systemNameSearchTerm = EditorGUILayout.TextField("Search", _systemNameSearchTerm);

                    const string clearButtonControlName = "Clear Button";
                    GUI.SetNextControlName(clearButtonControlName);
                    if (GUILayout.Button("x", GUILayout.Width(19), GUILayout.Height(14)))
                    {
                        _systemNameSearchTerm = string.Empty;
                        GUI.FocusControl(clearButtonControlName);
                    }
                }
                ProfilerEditorLayout.EndHorizontal();

                _showTickEngines = EditorGUILayout.Foldout(_showTickEngines, "Engines Ticks");
                if (_showTickEngines && ShouldShowSystems(engines))
                {
                    ProfilerEditorLayout.BeginVerticalBox();
                    {
                        var systemsDrawn = DrawUpdateEngineInfos(engines);
                        if (systemsDrawn == 0)
                        {
                            EditorGUILayout.LabelField(string.Empty);
                        }
                    }
                    ProfilerEditorLayout.EndVertical();
                }

                _showAddEngines = EditorGUILayout.Foldout(_showAddEngines, "Engines Add");
                if (_showAddEngines && ShouldShowSystems(engines))
                {
                    ProfilerEditorLayout.BeginVerticalBox();
                    {
                        var systemsDrawn = DrawAddEngineInfos(engines);
                        if (systemsDrawn == 0)
                        {
                            EditorGUILayout.LabelField(string.Empty);
                        }
                    }
                    ProfilerEditorLayout.EndVertical();
                }

                _showRemoveEngines = EditorGUILayout.Foldout(_showRemoveEngines, "Engines Remove");
                if (_showRemoveEngines && ShouldShowSystems(engines))
                {
                    ProfilerEditorLayout.BeginVerticalBox();
                    {
                        var systemsDrawn = DrawRemoveEngineInfos(engines);
                        if (systemsDrawn == 0)
                        {
                            EditorGUILayout.LabelField(string.Empty);
                        }
                    }
                    ProfilerEditorLayout.EndVertical();
                }
            }
            ProfilerEditorLayout.EndVertical();
        }

        void DrawEnginesMonitor(EngineInfo[] engines)
        {
            if (_enginesMonitor == null)
            {
                _enginesMonitor = new EnginesMonitor(SYSTEM_MONITOR_DATA_LENGTH);
                _engineMonitorData = new Queue<float>(new float[SYSTEM_MONITOR_DATA_LENGTH]);
                if (EditorApplication.update != Repaint)
                {
                    EditorApplication.update += Repaint;
                }
            }
            double totalDuration = 0;
            for (int i = 0; i < engines.Length; i++)
            {
                totalDuration += engines[i].lastUpdateDuration;
            }

            ProfilerEditorLayout.BeginVerticalBox();
            {
                EditorGUILayout.LabelField("Execution duration", EditorStyles.boldLabel);

                ProfilerEditorLayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Total", totalDuration.ToString());
                }
                ProfilerEditorLayout.EndHorizontal();

                ProfilerEditorLayout.BeginHorizontal();
                {
                    _axisUpperBounds = EditorGUILayout.FloatField("Axis Upper Bounds", _axisUpperBounds);
                }
                ProfilerEditorLayout.EndHorizontal();

                if (!EditorApplication.isPaused)
                {
                    if (_engineMonitorData.Count >= SYSTEM_MONITOR_DATA_LENGTH)
                    {
                        _engineMonitorData.Dequeue();
                    }

                    _engineMonitorData.Enqueue((float) totalDuration);
                }
                _enginesMonitor.Draw(_engineMonitorData.ToArray(), 80f, _axisUpperBounds);
            }
            ProfilerEditorLayout.EndVertical();
        }

        int DrawUpdateEngineInfos(EngineInfo[] engines)
        {
            if (_sortingOption != SORTING_OPTIONS.NONE)
            {
                SortUpdateEngines(engines);
            }

            string title =
                updateTitle.FastConcat(lateUpdateTitle)
                    .FastConcat(fixedupdateTitle)
                    .FastConcat(minTitle)
                    .FastConcat(maxTitle);
            EditorGUILayout.LabelField("Engine Name", title, EditorStyles.boldLabel);

            int enginesDrawn = 0;
            for (int i = 0; i < engines.Length; i++)
            {
                EngineInfo engineInfo = engines[i];

                if (engineInfo.engineName.ToLower().Contains(_systemNameSearchTerm.ToLower()))
                {
                    ProfilerEditorLayout.BeginHorizontal();
                    {
                        var avg = string.Format("{0:0.000}", engineInfo.averageUpdateDuration).PadRight(15);
                        var avgLate = string.Format("{0:0.000}", engineInfo.averageLateUpdateDuration).PadRight(15);
                        var avgFixed = string.Format("{0:0.000}", engineInfo.averageFixedUpdateDuration).PadRight(15);
                        var min = string.Format("{0:0.000}", engineInfo.minUpdateDuration).PadRight(15);
                        var max = string.Format("{0:0.000}", engineInfo.maxUpdateDuration);

                        string output = avg.FastConcat(avgLate).FastConcat(avgFixed).FastConcat(min).FastConcat(max);

                        EditorGUILayout.LabelField(engineInfo.engineName, output, GetEngineStyle());
                    }
                    ProfilerEditorLayout.EndHorizontal();

                    enginesDrawn += 1;
                }
            }
            return enginesDrawn;
        }

        int DrawAddEngineInfos(EngineInfo[] engines)
        {
            if (_sortingOption != SORTING_OPTIONS.NONE)
            {
                SortAddEngines(engines);
            }

            string title = avgTitle.FastConcat(minTitle).FastConcat(maxTitle);
            EditorGUILayout.LabelField("Engine Name", title, EditorStyles.boldLabel);

            int enginesDrawn = 0;
            for (int i = 0; i < engines.Length; i++)
            {
                EngineInfo engineInfo = engines[i];
                if (engineInfo.engineName.ToLower().Contains(_systemNameSearchTerm.ToLower()) &&
                    !engineInfo.minAddDuration.Equals(0) && !engineInfo.maxAddDuration.Equals(0))
                {
                    ProfilerEditorLayout.BeginHorizontal();
                    {
                        var avg = string.Format("{0:0.000}", engineInfo.averageAddDuration).PadRight(15);
                        var min = string.Format("{0:0.000}", engineInfo.minAddDuration).PadRight(15);
                        var max = string.Format("{0:0.000}", engineInfo.maxAddDuration);

                        string output = avg.FastConcat(min).FastConcat(max);

                        EditorGUILayout.LabelField(engineInfo.engineName, output, GetEngineStyle());
                    }
                    ProfilerEditorLayout.EndHorizontal();

                    enginesDrawn += 1;
                }
            }
            return enginesDrawn;
        }

        int DrawRemoveEngineInfos(EngineInfo[] engines)
        {
            if (_sortingOption != SORTING_OPTIONS.NONE)
            {
                SortRemoveEngines(engines);
            }

            string title = avgTitle.FastConcat(minTitle).FastConcat(maxTitle);
            EditorGUILayout.LabelField("Engine Name", title, EditorStyles.boldLabel);

            int enginesDrawn = 0;
            for (int i = 0; i < engines.Length; i++)
            {
                EngineInfo engineInfo = engines[i];
                if (engineInfo.engineName.ToLower().Contains(_systemNameSearchTerm.ToLower()) &&
                    !engineInfo.minRemoveDuration.Equals(0) && !engineInfo.maxRemoveDuration.Equals(0))
                {
                    ProfilerEditorLayout.BeginHorizontal();
                    {
                        var avg = string.Format("{0:0.000}", engineInfo.averageRemoveDuration).PadRight(15);
                        var min = string.Format("{0:0.000}", engineInfo.minRemoveDuration).PadRight(15);
                        var max = string.Format("{0:0.000}", engineInfo.maxRemoveDuration);

                        string output = avg.FastConcat(min).FastConcat(max);

                        EditorGUILayout.LabelField(engineInfo.engineName, output, GetEngineStyle());
                    }
                    ProfilerEditorLayout.EndHorizontal();

                    enginesDrawn += 1;
                }
            }
            return enginesDrawn;
        }

        static GUIStyle GetEngineStyle()
        {
            var style = new GUIStyle(GUI.skin.label);
            var color = EditorGUIUtility.isProSkin ? Color.white : style.normal.textColor;

            style.normal.textColor = color;

            return style;
        }

        static bool ShouldShowSystems(EngineInfo[] engines)
        {
            return engines.Length > 0;
        }

#region Sorting Engines
        void SortUpdateEngines(EngineInfo[] engines)
        {
            switch (_sortingOption)
            {
                case SORTING_OPTIONS.AVERAGE:
                    Array.Sort(engines,
                        (engine1, engine2) => engine2.averageUpdateDuration.CompareTo(engine1.averageUpdateDuration));
                    break;
                case SORTING_OPTIONS.MIN:
                    Array.Sort(engines,
                        (engine1, engine2) => engine2.minUpdateDuration.CompareTo(engine1.minUpdateDuration));
                    break;
                case SORTING_OPTIONS.MAX:
                    Array.Sort(engines,
                        (engine1, engine2) => engine2.maxUpdateDuration.CompareTo(engine1.maxUpdateDuration));
                    break;
                case SORTING_OPTIONS.NAME:
                    Array.Sort(engines, StringComparer.InvariantCulture);
                    break;
            }
        }

        void SortAddEngines(EngineInfo[] engines)
        {
            switch (_sortingOption)
            {
                case SORTING_OPTIONS.AVERAGE:
                    Array.Sort(engines,
                        (engine1, engine2) => engine2.averageAddDuration.CompareTo(engine1.averageAddDuration));
                    break;
                case SORTING_OPTIONS.MIN:
                    Array.Sort(engines,
                        (engine1, engine2) => engine2.minAddDuration.CompareTo(engine1.minAddDuration));
                    break;
                case SORTING_OPTIONS.MAX:
                    Array.Sort(engines,
                        (engine1, engine2) => engine2.maxAddDuration.CompareTo(engine1.maxAddDuration));
                    break;
                case SORTING_OPTIONS.NAME:
                    Array.Sort(engines, StringComparer.InvariantCulture);
                    break;
            }
        }

        void SortRemoveEngines(EngineInfo[] engines)
        {
            switch (_sortingOption)
            {
                case SORTING_OPTIONS.AVERAGE:
                    Array.Sort(engines,
                        (engine1, engine2) => engine2.averageRemoveDuration.CompareTo(engine1.averageRemoveDuration));
                    break;
                case SORTING_OPTIONS.MIN:
                    Array.Sort(engines,
                        (engine1, engine2) => engine2.minRemoveDuration.CompareTo(engine1.minRemoveDuration));
                    break;
                case SORTING_OPTIONS.MAX:
                    Array.Sort(engines,
                        (engine1, engine2) => engine2.maxRemoveDuration.CompareTo(engine1.maxRemoveDuration));
                    break;
                case SORTING_OPTIONS.NAME:
                    Array.Sort(engines, StringComparer.InvariantCulture);
                    break;
            }
        }
    }
#endregion
}
