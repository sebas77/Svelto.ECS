#if UNITY_EDITOR

using System;
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
        static bool _showAddEngines;
        static bool _showRemoveEngines;

        static string _systemNameSearchTerm = string.Empty;

        string minTitle = "Min".PadRight(15, ' ');
        string maxTitle = "Max".PadRight(15, ' ');
        string avgTitle = "Avg".PadRight(15, ' ');

        SORTING_OPTIONS _sortingOption = SORTING_OPTIONS.AVERAGE;

        public override void OnInspectorGUI()
        {
            var engineProfilerBehaviour = (EngineProfilerBehaviour) target;
            EngineInfo[] engines = new EngineInfo[engineProfilerBehaviour.engines.Count];
            engineProfilerBehaviour.engines.CopyTo(engines, 0);

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
                    Array.Sort(engines,
                        (engine1, engine2) => String.Compare(engine1.engineName, engine2.engineName, StringComparison.Ordinal));
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
                    Array.Sort(engines,
                        (engine1, engine2) => String.Compare(engine1.engineName, engine2.engineName, StringComparison.Ordinal));
                    break;
            }
        }
    }
#endregion
}
#endif