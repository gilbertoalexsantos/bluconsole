using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BluConsole.Extensions;
using BluConsole.Core;
using BluConsole.Core.UnityLoggerApi;


namespace BluConsole.Editor
{

    [Serializable]
    public class BluToolbarWindow 
    {

        private string _searchString = "";
        private bool _isClearOnPlay;
        private bool _isPauseOnError;
        private bool _isCollapse;
        private bool _isShowNormal;
        private bool _isShowWarning;
        private bool _isShowError;

        public List<bool> ToggledFilters = new List<bool>();
        public string[] SearchStringPatterns;

        public BluLogSettings Settings { get; set; }
        public BluLogConfiguration Configuration { get; set; }
        public BluListWindow ListWindow { get; set; }
        public BluDetailWindow DetailWindow { get; set; }
        public bool HasSetDirtyLogs { get; set; }
        public bool HasSetDirtyComparer { get; set; }

        public void OnGUI(int id)
        {
            float height = BluConsoleSkin.ToolbarButtonStyle.CalcSize("Clear".GUIContent()).y;
            
            GUILayout.BeginHorizontal(BluConsoleSkin.ToolbarStyle, GUILayout.Height(height));

            if (GetButtonClamped("Clear".GUIContent(), BluConsoleSkin.ToolbarButtonStyle))
            {
                UnityLoggerServer.Clear();
                SetDirtyLogs();
                ListWindow.SelectedMessage = -1;
                DetailWindow.SelectedMessage = -1;
            }

            GUILayout.Space(6.0f);

            var actualCollapse = UnityLoggerServer.HasFlag(ConsoleWindowFlag.Collapse);
            var newCollapse = GetToggleClamped(_isCollapse, "Collapse".GUIContent(), BluConsoleSkin.ToolbarButtonStyle);
            if (newCollapse != _isCollapse)
            {
                SetDirtyLogs();
                _isCollapse = newCollapse;
                UnityLoggerServer.SetFlag(ConsoleWindowFlag.Collapse, newCollapse);
            }
            else if (newCollapse != actualCollapse)
            {
                SetDirtyLogs();
                _isCollapse = actualCollapse;
            }


            var actualClearOnPlay = UnityLoggerServer.HasFlag(ConsoleWindowFlag.ClearOnPlay);
            var newClearOnPlay = GetToggleClamped(_isClearOnPlay, "Clear on Play".GUIContent(), BluConsoleSkin.ToolbarButtonStyle);
            if (newClearOnPlay != _isClearOnPlay)
            {
                _isClearOnPlay = newClearOnPlay;
                UnityLoggerServer.SetFlag(ConsoleWindowFlag.ClearOnPlay, newClearOnPlay);
            }
            else if (newClearOnPlay != actualClearOnPlay)
            {
                _isClearOnPlay = actualClearOnPlay;
            }


            var actualPauseOnError = UnityLoggerServer.HasFlag(ConsoleWindowFlag.ErrorPause);
            var newPauseOnError = GetToggleClamped(_isPauseOnError, "Pause on Error".GUIContent(), BluConsoleSkin.ToolbarButtonStyle);
            if (newPauseOnError != _isPauseOnError)
            {
                _isPauseOnError = newPauseOnError;
                UnityLoggerServer.SetFlag(ConsoleWindowFlag.ErrorPause, newPauseOnError);
            }
            else if (newPauseOnError != actualPauseOnError)
            {
                _isPauseOnError = actualPauseOnError;
            }


            GUILayout.FlexibleSpace();

            
            // Search Area
            var oldString = _searchString;
            _searchString = EditorGUILayout.TextArea(_searchString,
                                                     BluConsoleSkin.ToolbarSearchTextFieldStyle,
                                                     GUILayout.Width(Configuration.SearchStringBoxWidth));
            if (_searchString != oldString)
                SetDirtyComparer();

            if (GUILayout.Button("", BluConsoleSkin.ToolbarSearchCancelButtonStyle))
            {
                _searchString = "";
                SetDirtyComparer();
                GUI.FocusControl(null);
            }

            SearchStringPatterns = _searchString.Trim().ToLower().Split(' ');

            
            GUILayout.Space(10.0f);


            // Info/Warning/Error buttons Area
            int qtNormalLogs = 0, qtWarningLogs = 0, qtErrorLogs = 0;
            UnityLoggerServer.GetCount(ref qtNormalLogs, ref qtWarningLogs, ref qtErrorLogs);

            var qtNormalLogsStr = qtNormalLogs.ToString();
            if (qtNormalLogs >= Configuration.MaxAmountOfLogs)
                qtNormalLogsStr = Configuration.MaxAmountOfLogs + "+";

            var qtWarningLogsStr = qtWarningLogs.ToString();
            if (qtWarningLogs >= Configuration.MaxAmountOfLogs)
                qtWarningLogsStr = Configuration.MaxAmountOfLogs + "+";

            var qtErrorLogsStr = qtErrorLogs.ToString();
            if (qtErrorLogs >= Configuration.MaxAmountOfLogs)
                qtErrorLogsStr = Configuration.MaxAmountOfLogs + "+";


            var actualIsShowNormal = UnityLoggerServer.HasFlag(ConsoleWindowFlag.LogLevelLog);
            var newIsShowNormal = GetToggleClamped(_isShowNormal,
                                                    new GUIContent(qtNormalLogsStr, BluConsoleSkin.InfoIconSmall),
                                                    BluConsoleSkin.ToolbarButtonStyle);
            if (newIsShowNormal != _isShowNormal)
            {
                SetDirtyLogs();
                _isShowNormal = newIsShowNormal;
                UnityLoggerServer.SetFlag(ConsoleWindowFlag.LogLevelLog, newIsShowNormal);
            }
            else if (newIsShowNormal != actualIsShowNormal)
            {
                SetDirtyLogs();
                _isShowNormal = actualIsShowNormal;
            }


            var actualIsShowWarning = UnityLoggerServer.HasFlag(ConsoleWindowFlag.LogLevelWarning);
            var newIsShowWarning = GetToggleClamped(_isShowWarning,
                                                    new GUIContent(qtWarningLogsStr, BluConsoleSkin.WarningIconSmall),
                                                    BluConsoleSkin.ToolbarButtonStyle);
            if (newIsShowWarning != _isShowWarning)
            {
                SetDirtyLogs();
                _isShowWarning = newIsShowWarning;
                UnityLoggerServer.SetFlag(ConsoleWindowFlag.LogLevelWarning, newIsShowWarning);
            }
            else if (newIsShowWarning != actualIsShowWarning)
            {
                SetDirtyLogs();
                _isShowWarning = actualIsShowWarning;
            }


            var actualIsShowError = UnityLoggerServer.HasFlag(ConsoleWindowFlag.LogLevelError);
            var newIsShowError = GetToggleClamped(_isShowError,
                                                  new GUIContent(qtErrorLogsStr, BluConsoleSkin.ErrorIconSmall),
                                                  BluConsoleSkin.ToolbarButtonStyle);
            if (newIsShowError != _isShowError)
            {
                SetDirtyLogs();
                _isShowError = newIsShowError;
                UnityLoggerServer.SetFlag(ConsoleWindowFlag.LogLevelError, newIsShowError);
            }
            else if (newIsShowError != actualIsShowError)
            {
                SetDirtyLogs();
                _isShowError = actualIsShowError;
            }

            for (var i = 0; i < Settings.Filters.Count; i++)
            {
                var name = Settings.Filters[i].Name;
                var style = BluConsoleSkin.ToolbarButtonStyle;
                bool oldAdditionalFilter = ToggledFilters[i];
                ToggledFilters[i] = GUILayout.Toggle(ToggledFilters[i],
                                                     name,
                                                     style,
                                                     GUILayout.MaxWidth(style.CalcSize(new GUIContent(name)).x));
                if (oldAdditionalFilter != ToggledFilters[i])
                    SetDirtyLogs();
            }

            GUILayout.EndHorizontal();
        }

        private void SetDirtyLogs()
        {
            HasSetDirtyLogs = true;
        }

        private void SetDirtyComparer()
        {
            HasSetDirtyComparer = true;
        }

        private bool GetButtonClamped(GUIContent content, GUIStyle style)
        {
            return GUILayout.Button(content, style, GUILayout.MaxWidth(style.CalcSize(new GUIContent(content)).x));
        }

        private bool GetToggleClamped(bool state, GUIContent content, GUIStyle style)
        {
            return GUILayout.Toggle(state, content, style, GUILayout.MaxWidth(style.CalcSize(content).x));
        }        
        
    }

}
