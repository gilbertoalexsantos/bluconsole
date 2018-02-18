using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluConsole.Core;
using BluConsole.Core.UnityLoggerApi;
using BluConsole.Extensions;


namespace BluConsole.Editor
{

    public class BluConsoleEditorWindow : EditorWindow, IHasCustomMenu
    {
        
        #region Variables

        // Cache Variables
        private UnityApiEvents _unityApiEvents;
        private BluLogSettings _settings;
        private BluLogConfiguration _configuration;
        private List<BluLog> _cacheLog = new List<BluLog>();
        private List<bool> _cacheLogComparer = new List<bool>();
        private List<string> _stackTraceIgnorePrefixs = new List<string>();
        private int[] _cacheIntArr = new int[100];
        private BluLog[] _cacheLogArr = new BluLog[100];
        private int _cacheLogCount;
        private int _qtLogs;

        // Toolbar Variables
        private string[] _searchStringPatterns;
        private string _searchString = "";
        private bool _isClearOnPlay;
        private bool _isPauseOnError;
        private bool _isCollapse;
        private bool _isShowNormal;
        private bool _isShowWarning;
        private bool _isShowError;

        // Resizer Variables
        private float _topPanelHeight;
        private Rect _cursorChangeRect;
        private bool _isResizing;

        // LogDetail Variables
        private Vector2 _logDetailScrollPosition;
        private int _logDetailSelectedFrame = -1;
        private double _logDetailLastTimeClicked;
        private BluLog _selectedLog;

        // Filter Variables
        private List<bool> _toggledFilters = new List<bool>();

        // Keyboard Controll Variables
        private bool _hasKeyboardArrowKeyInput;
        private Direction _logMoveDirection;
        private ClickContext _clickContext;

        public BluListWindow ListWindow { get; set; }
        public BluDetailWindow DetailWindow { get; set; }

        #endregion Variables

        
        #region Windows
        
        [MenuItem("Window/BluConsole")]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<BluConsoleEditorWindow>("BluConsole");

            var consoleIcon = BluConsoleSkin.ConsoleIcon;
            if (consoleIcon != null)
                window.titleContent = new GUIContent("BluConsole", consoleIcon);
            else
                window.titleContent = new GUIContent("BluConsole");

            window._topPanelHeight = window.position.height / 2.0f;
            window.ListWindow = new BluListWindow();
        }
        
        #endregion Windows
        
        
        #region OnEvents

        private void OnEnable()
        {
            _stackTraceIgnorePrefixs = BluUtils.StackTraceIgnorePrefixs;
            _settings = BluLogSettings.Instance;
            _configuration = BluLogConfiguration.Instance;
            _unityApiEvents = UnityApiEvents.Instance;
            SetDirtyLogs();
        }

        private void OnDestroy()
        {
            _settings = null;
            _unityApiEvents = null;
            Resources.UnloadAsset(titleContent.image);
            Resources.UnloadUnusedAssets();
        }

        private void Update()
        {
            _unityApiEvents.OnBeforeCompileEvent.AddCallback(SetDirtyLogs);
            _unityApiEvents.OnAfterCompileEvent.AddCallback(OnAfterCompile);
            _unityApiEvents.OnBeginPlayEvent.AddCallback(SetDirtyLogs);
            _unityApiEvents.OnStopPlayEvent.AddCallback(SetDirtyLogs);
        }

        private void OnGUI()
        {
            InitVariables();

            CalculateResizer();

            DrawYPos += DrawTopToolbar();
            DrawYPos -= 1f;
            
            _qtLogs = UnityLoggerServer.StartGettingLogs();
            PreProcessLogs();

            BeginWindows();
            DrawYPos += DrawListWindow(id: 1);
            DrawYPos += DrawResizer();
            DrawYPos += DrawDetailWindow(id: 2);
            EndWindows();

            UnityLoggerServer.StopGettingsLogs();

            Repaint();
        }

        private void OnAfterCompile()
        {
            SetDirtyLogs();
            ListWindow.SelectedMessage = -1;
            _logDetailSelectedFrame = -1;
        }
        
        #endregion OnEvents


        #region IHasCustomMenu

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Add Filter"), false, OnAddFilterTabClicked);
        }

        private void OnAddFilterTabClicked()
        {
            if (_settings == null)
                return;
            Selection.activeObject = _settings;
        }

        #endregion IHasCustomMenu

        
        #region Draw

        private float DrawTopToolbar()
        {
            float height = BluConsoleSkin.ToolbarButtonStyle.CalcSize("Clear".GUIContent()).y;
            
            GUILayout.BeginHorizontal(BluConsoleSkin.ToolbarStyle, GUILayout.Height(height));

            if (GetButtonClamped("Clear".GUIContent(), BluConsoleSkin.ToolbarButtonStyle))
            {
                UnityLoggerServer.Clear();
                SetDirtyLogs();
                ListWindow.SelectedMessage = -1;
                _logDetailSelectedFrame = -1;
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
                                                     GUILayout.Width(SearchStringBoxWidth));
            if (_searchString != oldString)
                SetDirtyComparer();

            if (GUILayout.Button("", BluConsoleSkin.ToolbarSearchCancelButtonStyle))
            {
                _searchString = "";
                SetDirtyComparer();
                GUI.FocusControl(null);
            }

            _searchStringPatterns = _searchString.Trim().ToLower().Split(' ');

            
            GUILayout.Space(10.0f);


            // Info/Warning/Error buttons Area
            int qtNormalLogs = 0, qtWarningLogs = 0, qtErrorLogs = 0;
            UnityLoggerServer.GetCount(ref qtNormalLogs, ref qtWarningLogs, ref qtErrorLogs);

            var qtNormalLogsStr = qtNormalLogs.ToString();
            if (qtNormalLogs >= MaxAmountOfLogs)
                qtNormalLogsStr = MaxAmountOfLogs + "+";

            var qtWarningLogsStr = qtWarningLogs.ToString();
            if (qtWarningLogs >= MaxAmountOfLogs)
                qtWarningLogsStr = MaxAmountOfLogs + "+";

            var qtErrorLogsStr = qtErrorLogs.ToString();
            if (qtErrorLogs >= MaxAmountOfLogs)
                qtErrorLogsStr = MaxAmountOfLogs + "+";


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

            for (var i = 0; i < _settings.Filters.Count; i++)
            {
                var name = _settings.Filters[i].Name;
                var style = BluConsoleSkin.ToolbarButtonStyle;
                bool oldAdditionalFilter = _toggledFilters[i];
                _toggledFilters[i] = GUILayout.Toggle(_toggledFilters[i],
                                                     name,
                                                     style,
                                                     GUILayout.MaxWidth(style.CalcSize(new GUIContent(name)).x));
                if (oldAdditionalFilter != _toggledFilters[i])
                    SetDirtyLogs();
            }

            GUILayout.EndHorizontal();

            return height;
        }

        private float DrawListWindow(int id)
        {
            Rect originalRect = new Rect(0, DrawYPos, WindowWidth, _topPanelHeight - DrawYPos);

            ListWindow.WindowRect = new Rect(0, 0, originalRect.width, originalRect.height);
            ListWindow.Rows = GetCachedIntArr(_qtLogs);
            ListWindow.Logs = GetCachedLogsArr(_qtLogs);
            ListWindow.LogConfiguration = _configuration;
            ListWindow.QtLogs = _qtLogs;
            ListWindow.StackTraceIgnorePrefixs = _stackTraceIgnorePrefixs;

            GUI.Window(id, originalRect, ListWindow.OnGUI, GUIContent.none, GUIStyle.none);

            return originalRect.height;
        }

        private float DrawDetailWindow(int id)
        {
            Rect originalRect = new Rect(0, DrawYPos, WindowWidth, WindowHeight - DrawYPos);

            DetailWindow.WindowRect = new Rect(0, 0, originalRect.width, originalRect.height);
            DetailWindow.Rows = GetCachedIntArr(_qtLogs);
            DetailWindow.Logs = GetCachedLogsArr(_qtLogs);
            DetailWindow.LogConfiguration = _configuration;
            DetailWindow.QtLogs = _qtLogs;
            DetailWindow.StackTraceIgnorePrefixs = _stackTraceIgnorePrefixs;
            DetailWindow.ListWindowSelectedMessage = ListWindow.SelectedMessage;

            GUI.Window(id, originalRect, DetailWindow.OnGUI, GUIContent.none, GUIStyle.none);

            return originalRect.height;
        }

        private float DrawResizer()
        {
            if (!IsRepaintEvent)
                return ResizerHeight;
            
            var rect = new Rect(0, DrawYPos, position.width, ResizerHeight);
            EditorGUI.DrawRect(rect, ResizerColor);

            return ResizerHeight;
        }

        private void DrawPopup(Event clickEvent, BluLog log)
        {
            GenericMenu.MenuFunction copyCallback = () => { EditorGUIUtility.systemCopyBuffer = log.Message; };

            GenericMenu menu = new GenericMenu();
            menu.AddItem(content: "Copy".GUIContent(), on: false, func: copyCallback);
            menu.ShowAsContext();

            clickEvent.Use();
        }

        private void DrawBackground(Rect rect, GUIStyle style, bool isSelected)
        {
            if (IsRepaintEvent)
                style.Draw(rect, false, false, isSelected, false);
        }

        #endregion Draw

        
        #region Gets

        private int[] GetCachedIntArr(int size)
        {
            if (size > _cacheIntArr.Length)
                _cacheIntArr = new int[size * 2];
            return _cacheIntArr;
        }

        private BluLog[] GetCachedLogsArr(int size)
        {
            if (size > _cacheLogArr.Length)
                _cacheLogArr = new BluLog[size * 2];
            return _cacheLogArr;
        }

        private BluLog GetSimpleLog(int row)
        {
            int realCount = _cacheLog.Count;
            var log = UnityLoggerServer.GetSimpleLog(row);
            if (realCount > row)
                _cacheLog[row] = log;
            else
                _cacheLog.Add(log);
            return _cacheLog[row];
        }

        private BluLog GetCompleteLog(int row)
        {
            var log = UnityLoggerServer.GetCompleteLog(row);
            log.FilterStackTrace(_stackTraceIgnorePrefixs);
            return log;
        }

        private string GetTruncatedMessage(string message)
        {
            if (message.Length <= MaxLengthMessage)
                return message;

            return string.Format("{0}... <truncated>", message.Substring(startIndex: 0, length: MaxLengthMessage));
        }

        private string GetLogListMessage(BluLog log)
        {
            return log.Message.Replace(System.Environment.NewLine, " ");
        }

        private GUIStyle GetLogBackStyle(int row)
        {
            return row % 2 == 0 ? BluConsoleSkin.EvenBackStyle : BluConsoleSkin.OddBackStyle;
        }

        private float GetDetailMessageHeight(string message, GUIStyle style, float width = 0f)
        {
            return style.CalcHeight(new GUIContent(message), width);
        }

        private bool GetButtonClamped(GUIContent content, GUIStyle style)
        {
            return GUILayout.Button(content, style, GUILayout.MaxWidth(style.CalcSize(new GUIContent(content)).x));
        }

        private bool GetToggleClamped(bool state, GUIContent content, GUIStyle style)
        {
            return GUILayout.Toggle(state, content, style, GUILayout.MaxWidth(style.CalcSize(content).x));
        }

        private bool IsClicked(Rect rect)
        {
            return Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition);
        }
        
        private bool ShouldLog(BluLog log, int row)
        {
            var messageLower = log.MessageLower;

            if (_searchStringPatterns.Any(pattern => !messageLower.Contains(pattern)))
            {
                CacheLogComparer(row, false);
                return false;
            }

            var hasPattern = true;
            for (var i = 0; i < _settings.Filters.Count; i++)
            {
                if (!_toggledFilters[i])
                    continue;

                if (!_settings.Filters[i].Patterns.Any(pattern => messageLower.Contains(pattern)))
                {
                    hasPattern = false;
                    break;
                }
            }

            CacheLogComparer(row, hasPattern);
            return hasPattern;
        }

        #endregion Gets
        
        
        #region Sets
        
        private void SetDirtyComparer()
        {
            _cacheLogComparer.Clear();
        }

        private void SetDirtyLogs()
        {
            _cacheLog.Clear();
            _cacheLogCount = 0;
            _cacheIntArr = new int[50];
            _cacheLogArr = new BluLog[50];
            SetDirtyComparer();
        }
        
        #endregion Sets

        
        #region Actions

        private void CalculateResizer()
        {
            var resizerY = _topPanelHeight;

            _cursorChangeRect = new Rect(0, resizerY - 2f, position.width, ResizerHeight + 3f);

            EditorGUIUtility.AddCursorRect(_cursorChangeRect, MouseCursor.ResizeVertical);

            if (IsClicked(_cursorChangeRect))
                _isResizing = true;
            else if (Event.current.rawType == EventType.MouseUp)
                _isResizing = false;

            if (_isResizing)
            {
                _topPanelHeight = Event.current.mousePosition.y;
                _cursorChangeRect.Set(_cursorChangeRect.x, resizerY, _cursorChangeRect.width, _cursorChangeRect.height);
            }

            _topPanelHeight = Mathf.Clamp(_topPanelHeight, MinHeightOfTopAndBottom, position.height - MinHeightOfTopAndBottom);
        }

        private void HandleKeyboardEnterKey()
        {
            Event e = Event.current;

            if (e == null || !e.isKey || e.type != EventType.KeyUp || e.keyCode != KeyCode.Return)
                return;
            
            if (_selectedLog == null)
                return;

            switch (_clickContext)
            {
                case ClickContext.List:
                    JumpToSourceFile(_selectedLog, 0);
                    break;
                case ClickContext.Detail:
                    JumpToSourceFile(_selectedLog, _logDetailSelectedFrame < 0 ? 0 : _logDetailSelectedFrame);
                    break;
            }
        }

        private void HandleKeyboardArrowKeys()
        {
            Event e = Event.current;
            _hasKeyboardArrowKeyInput = false;
            _logMoveDirection = Direction.None;
            
            if (e == null || e.type != EventType.KeyDown || !e.isKey) 
                return;
            
            var refresh = false;
            switch (e.keyCode)
            {
                case KeyCode.UpArrow:
                    refresh = true;
                    _logMoveDirection = Direction.Up;
                    MoveLogPosition(Direction.Up, _clickContext);
                    break;
                case KeyCode.DownArrow:
                    refresh = true;
                    _logMoveDirection = Direction.Down;
                    MoveLogPosition(Direction.Down, _clickContext);
                    break;
            }

            if (!refresh)
                return;

            _hasKeyboardArrowKeyInput = true;
            ListWindow.SelectedMessage = Mathf.Clamp(ListWindow.SelectedMessage, 0, _qtLogs - 1);
            if (_selectedLog != null)
            {
                if (_logDetailSelectedFrame < -2)
                    _logDetailSelectedFrame = -2;
                else if (_logDetailSelectedFrame >= _selectedLog.StackTrace.Count)
                    _logDetailSelectedFrame = _selectedLog.StackTrace.Count - 1;
            }
        }

        private void MoveLogPosition(Direction direction, ClickContext context)
        {
            switch (context)
            {
                case ClickContext.List:
                    ListWindow.SelectedMessage += direction == Direction.Up ? -1 : +1;
                    break;
                case ClickContext.Detail:
                    if (_logDetailSelectedFrame == -2)
                        _logDetailSelectedFrame = direction == Direction.Up ? -2 : 0;
                    else if (_logDetailSelectedFrame == 0)
                        _logDetailSelectedFrame = direction == Direction.Up ? -2 : 1;
                    else
                        _logDetailSelectedFrame += direction == Direction.Up ? -1 : +1;
                    break;
            }
        }

        private void PingLog(BluLog log)
        {
            if (log.InstanceID != 0)
                EditorGUIUtility.PingObject(log.InstanceID);
        }

        private void CacheLogComparer(int row, bool value)
        {
            if (row < _cacheLogComparer.Count)
                _cacheLogComparer[row] = value;
            else
                _cacheLogComparer.Add(value);
        }

        private void JumpToSourceFile(BluLog log, int row)
        {
            var file = "";
            var line = -1;

            if (log.StackTrace.Count == 0)
            {
                file = log.File;
                line = log.Line;
            }
            else if (row < log.StackTrace.Count)
            {
                file = log.StackTrace[row].File;
                line = log.StackTrace[row].Line;
            }

            if (string.IsNullOrEmpty(file) || line == -1)
                return;

            BluUtils.OpenFileOnEditor(file, line);
        }

        private void InitVariables()
        {
            while (_toggledFilters.Count < _settings.Filters.Count)
                _toggledFilters.Add(false);
            DefaultButtonWidth = position.width;
            DefaultButtonHeight = BluConsoleSkin.MessageStyle.CalcSize("Test".GUIContent()).y + DefaultButtonHeightOffset;
            DrawYPos = 0f;
        }

        private void PreProcessLogs()
        {
            _cacheLogCount = _qtLogs;

            // Filtering logs with ugly code
            int cntLogs = 0;
            int[] rows = GetCachedIntArr(_qtLogs);
            BluLog[] logs = GetCachedLogsArr(_qtLogs);
            int index = 0;
            int cacheLogComparerCount = _cacheLogComparer.Count;
            for (int i = 0; i < _qtLogs; i++)
            {
                // Ugly code to avoid function call 
                int realCount = _cacheLog.Count;
                BluLog log = null;
                if (i < _cacheLogCount && i < realCount)
                    log = _cacheLog[i];
                else
                    log = GetSimpleLog(i);

                // Ugly code to avoid function call 
                bool has = false;
                if (i < cacheLogComparerCount)
                    has = _cacheLogComparer[i];
                else
                    has = ShouldLog(log, i);
                if (has)
                {
                    cntLogs++;
                    rows[index] = i;
                    logs[index++] = log;
                }
            }

            _qtLogs = cntLogs;
        }

        #endregion Actions

        
        #region Properties

        private bool IsScrollUp
        {
            get
            {
                return Event.current.type == EventType.ScrollWheel && Event.current.delta.y < 0f;
            }
        }

        private bool IsRepaintEvent
        {
            get
            {
                return Event.current.type == EventType.Repaint;
            }
        }

        private bool IsDoubleClickLogDetailButton
        {
            get
            {
                return (EditorApplication.timeSinceStartup - _logDetailLastTimeClicked) < 0.3f && !_hasKeyboardArrowKeyInput;
            }
        }

        private float WindowWidth { get { return position.width; } }
        private float WindowHeight { get { return position.height; } }

        private bool IsFollowScroll { get; set; }
        private float DrawYPos { get; set; }
        private float DefaultButtonWidth { get; set; }
        private float DefaultButtonHeight { get; set; }
        
        private float ResizerHeight { get { return 1.0f; } }
        private float MinHeightOfTopAndBottom { get { return 60.0f; } }
        private float DefaultButtonHeightOffset { get { return 15.0f; } }
        private int MaxLengthMessage { get { return 999; } }
        private int MaxAmountOfLogs { get { return 999; } }
        private int MaxLengthtoCollapse { get { return 999; } }
        private Color ResizerColor { get { return Color.black; } }
        private float SearchStringBoxWidth { get { return 200.0f; } }
        
        #endregion Properties

    }

}
