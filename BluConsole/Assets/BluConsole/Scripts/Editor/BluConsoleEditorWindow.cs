using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluConsole.Core;
using BluConsole.Core.UnityLoggerApi;
using BluConsole.Extensions;


namespace BluConsole.Editor
{

    public enum ClickContext
    {
        None = 0,
        List = 1,
        Detail = 2
    }

    public enum Direction
    {
        None = 0,
        Up   = 1,
        Down = 2
    }

    public class BluConsoleEditorWindow : EditorWindow, IHasCustomMenu
    {
        
        #region Variables

        // Cache Variables
        private UnityApiEvents _unityApiEvents;
        private BluLogSettings _settings;
        private List<BluLog> _cacheLog = new List<BluLog>();
        private List<bool> _cacheLogComparer = new List<bool>();
        private List<string> _stackTraceIgnorePrefixs = new List<string>();
        private int[] _cacheIntArr = new int[100];
        private BluLog[] _cacheLogArr = new BluLog[100];
        private int _cacheLogCount;

        // Toolbar Variables
        private string[] _searchStringPatterns;
        private string _searchString = "";
        private bool _isClearOnPlay;
        private bool _isPauseOnError;
        private bool _isCollapse;
        private bool _isShowNormal;
        private bool _isShowWarning;
        private bool _isShowError;

        // LogList Variables
        private Vector2 _logListScrollPosition;
        private int _logListSelectedMessage = -1;
        private double _logListLastTimeClicked;
        private int _qtLogs;

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
        }
        
        #endregion Windows
        
        
        #region OnEvents

        private void OnEnable()
        {
            _stackTraceIgnorePrefixs = BluUtils.StackTraceIgnorePrefixs;
            _settings = BluLogSettings.Instance;
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

            HandleKeyboardArrowKeys();
            HandleKeyboardEnterKey();

            CalculateResizer();

            DrawYPos += DrawTopToolbar();
            DrawYPos -= 1f;
            
            DrawYPos += DrawLogList();
            DrawYPos += DrawResizer();
            DrawYPos += DrawLogDetail();

            Repaint();
        }

        private void OnAfterCompile()
        {
            SetDirtyLogs();
            _logListSelectedMessage = -1;
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
                _logListSelectedMessage = -1;
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

        private float DrawLogList()
        {
            _qtLogs = UnityLoggerServer.StartGettingLogs();
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

            float windowWidth = WindowWidth;
            float windowHeight = _topPanelHeight - DrawYPos;

            float buttonWidth = DefaultButtonWidth;
            
            // If the amount of logs is greater than the window height, then the ScrollBar will appear. 
            // We need to decrease the ScrollBar width then.
            if (_qtLogs * DefaultButtonHeight > windowHeight)
                buttonWidth -= 15f;

            float viewWidth = buttonWidth;
            float viewHeight = _qtLogs * DefaultButtonHeight;

            Rect scrollWindowRect = new Rect(x: 0f, y: DrawYPos, width: windowWidth, height: windowHeight);
            Rect scrollViewRect = new Rect(x: 0f, y: 0f, width: viewWidth, height: viewHeight);

            GUI.DrawTexture(scrollWindowRect, BluConsoleSkin.EvenBackTexture);

            Vector2 oldScrollPosition = _logListScrollPosition;
            _logListScrollPosition = GUI.BeginScrollView(position: scrollWindowRect,
                                                         scrollPosition: _logListScrollPosition,
                                                         viewRect: scrollViewRect);

            int firstRenderLogIndex = (int)(_logListScrollPosition.y / DefaultButtonHeight);
            firstRenderLogIndex = Mathf.Clamp(firstRenderLogIndex, 0, _qtLogs);

            int lastRenderLogIndex = firstRenderLogIndex + (int)(windowHeight / DefaultButtonHeight) + 2;
            lastRenderLogIndex = Mathf.Clamp(lastRenderLogIndex, 0, _qtLogs);

            
            // Handling up/down arrow keys
            if (_hasKeyboardArrowKeyInput && _clickContext == ClickContext.List)
            {
                bool isFrameOutsideOfRange = _logListSelectedMessage < firstRenderLogIndex + 1 ||
                                             _logListSelectedMessage > lastRenderLogIndex - 3;
                if (isFrameOutsideOfRange && _logMoveDirection == Direction.Up)
                {
                    _logListScrollPosition.y = DefaultButtonHeight * _logListSelectedMessage;
                }
                else if (isFrameOutsideOfRange && _logMoveDirection == Direction.Down)
                {
                    int md = lastRenderLogIndex - firstRenderLogIndex - 3;
                    float ss = md * DefaultButtonHeight;
                    float sd = windowHeight - ss;
                    _logListScrollPosition.y = (DefaultButtonHeight * (_logListSelectedMessage + 1) - ss - sd);
                }
            }

            
            float buttonY = firstRenderLogIndex * DefaultButtonHeight;
            bool hasSomeClick = false;

            bool hasCollapse = UnityLoggerServer.HasFlag(ConsoleWindowFlag.Collapse);
            for (int i = firstRenderLogIndex; i < lastRenderLogIndex; i++)
            {
                var row = rows[i];
                var log = logs[i];
                var styleBack = GetLogBackStyle(i);

                var styleMessage = BluConsoleSkin.GetLogListStyle(log.LogType);
                string showMessage = GetTruncatedMessage(GetLogListMessage(log));
                var contentMessage = new GUIContent(showMessage);
                var rectMessage = new Rect(x: 0, y: buttonY, width: viewWidth, height: DefaultButtonHeight);
                bool isSelected = i == _logListSelectedMessage;
                
                DrawBackground(rectMessage, styleBack, isSelected);
                if (IsRepaintEvent)
                    styleMessage.Draw(rectMessage, contentMessage, false, false, isSelected, false);

                bool messageClicked = IsClicked(rectMessage);
                bool isLeftClick = messageClicked && Event.current.button == 0;

                if (hasCollapse)
                {
                    int quantity = UnityLoggerServer.GetLogCount(row);
                    var collapseCount = Mathf.Min(quantity, MaxLengthtoCollapse);
                    var collapseText = collapseCount.ToString();
                    if (collapseCount >= MaxLengthtoCollapse)
                        collapseText += "+";
                    var collapseContent = new GUIContent(collapseText);
                    var collapseSize = BluConsoleSkin.CollapseStyle.CalcSize(collapseContent);

                    var collapseRect = new Rect(x: viewWidth - collapseSize.x - 5f,
                                                y: (buttonY + buttonY + DefaultButtonHeight - collapseSize.y) * 0.5f,
                                                width: collapseSize.x,
                                                height: collapseSize.y);

                    GUI.Label(collapseRect, collapseContent, BluConsoleSkin.CollapseStyle);
                }

                if (messageClicked)
                {
                    _clickContext = ClickContext.List;
                    if (isLeftClick)
                        _selectedLog = GetCompleteLog(row);
                    hasSomeClick = true;
                    if (_logListSelectedMessage != i)
                        _logListLastTimeClicked = 0.0f;
                    if (isLeftClick && !IsDoubleClickLogDetailButton)
                    {
                        PingLog(_selectedLog);
                        _logListSelectedMessage = i;
                    }
                    if (!isLeftClick)
                        DrawPopup(Event.current, log);
                    if (isLeftClick && i == _logListSelectedMessage)
                    {
                        if (IsDoubleClickLogListButton)
                        {
                            _logListLastTimeClicked = 0.0f;
                            var completeLog = GetCompleteLog(row);
                            JumpToSourceFile(completeLog, 0);
                        }
                        else
                            _logListLastTimeClicked = EditorApplication.timeSinceStartup;
                    }
                    if (isLeftClick)
                        _logDetailSelectedFrame = -1;
                }

                buttonY += DefaultButtonHeight;
            }

            UnityLoggerServer.StopGettingsLogs();

            GUI.EndScrollView();

            if (IsScrollUp || hasSomeClick)
            {
                IsFollowScroll = false;
            }
            else if (_logListScrollPosition != oldScrollPosition)
            {
                IsFollowScroll = false;
                float topOffset = viewHeight - windowHeight;
                if (_logListScrollPosition.y >= topOffset)
                    IsFollowScroll = true;
            }

            if (!IsFollowScroll)
                return windowHeight;

            float endY = viewHeight - windowHeight;
            _logListScrollPosition.y = endY;

            return windowHeight;
        }

        private float DrawResizer()
        {
            if (!IsRepaintEvent)
                return ResizerHeight;
            
            var rect = new Rect(0, DrawYPos, position.width, ResizerHeight);
            EditorGUI.DrawRect(rect, ResizerColor);

            return ResizerHeight;
        }

        private float DrawLogDetail()
        {
            var windowHeight = WindowHeight - DrawYPos;

            {
                var rect = new Rect(x: 0, y: DrawYPos, width: WindowWidth, height: windowHeight);
                GUI.DrawTexture(rect, BluConsoleSkin.EvenBackTexture);
            }

            if (_logListSelectedMessage == -1 ||
                _qtLogs == 0 ||
                _logListSelectedMessage >= _qtLogs ||
                _selectedLog == null ||
                _selectedLog.StackTrace == null)
            {
                return windowHeight;
            }

            var log = _selectedLog;
            var size = log.StackTrace.Count;
            var sizePlus = size + 1;

            float buttonHeight = GetDetailMessageHeight("A", BluConsoleSkin.MessageDetailCallstackStyle);
            float buttonWidth = DefaultButtonWidth;
            float firstLogHeight = Mathf.Max(buttonHeight, GetDetailMessageHeight(GetTruncatedMessage(log.Message),
                                                                                  BluConsoleSkin.MessageDetailFirstLogStyle,
                                                                                  buttonWidth));

            float viewHeight = size * buttonHeight + firstLogHeight;

            if (viewHeight > windowHeight)
            {
                buttonWidth -= 15f;

                // Recalculate it because we decreased the buttonWidth
                firstLogHeight = Mathf.Max(buttonHeight, 
                                           GetDetailMessageHeight(GetTruncatedMessage(log.Message),
                                                                  BluConsoleSkin.MessageDetailFirstLogStyle,
                                                                  buttonWidth));
            }
            viewHeight = size * buttonHeight + firstLogHeight;

            float viewWidth = buttonWidth;

            Rect scrollViewPosition = new Rect(x: 0f, y: DrawYPos, width: WindowWidth, height: windowHeight);
            Rect scrollViewViewRect = new Rect(x: 0f, y: 0f, width: viewWidth, height: viewHeight);

            _logDetailScrollPosition = GUI.BeginScrollView(position: scrollViewPosition,
                                                           scrollPosition: _logDetailScrollPosition,
                                                           viewRect: scrollViewViewRect);

            // Return if has nothing to show
            if (_logListSelectedMessage == -1 || _qtLogs == 0 || _logListSelectedMessage >= _qtLogs)
            {
                GUI.EndScrollView();
                return windowHeight;
            }

            float scrollY = _logDetailScrollPosition.y;

            int firstRenderLogIndex = 0;
            if (scrollY <= firstLogHeight)
                firstRenderLogIndex = 0;
            else
                firstRenderLogIndex = (int)((scrollY - firstLogHeight) / buttonHeight) + 1;
            firstRenderLogIndex = Mathf.Clamp(firstRenderLogIndex, 0, sizePlus);

            int lastRenderLogIndex = 0;
            if (firstRenderLogIndex == 0)
            {
                float offsetOfFirstLog = firstLogHeight - scrollY;
                if (windowHeight > offsetOfFirstLog)
                    lastRenderLogIndex = firstRenderLogIndex + (int)((windowHeight - offsetOfFirstLog) / buttonHeight) + 2;
                else
                    lastRenderLogIndex = 2;
            }
            else
            {
                lastRenderLogIndex = firstRenderLogIndex + (int)(windowHeight / buttonHeight) + 2;
            }
            lastRenderLogIndex = Mathf.Clamp(lastRenderLogIndex, 0, sizePlus);

            float buttonY = 0f;
            if (firstRenderLogIndex > 0)
                buttonY = firstLogHeight + (firstRenderLogIndex - 1) * buttonHeight;

            
            // Handling up/down arrow keys
            if (_hasKeyboardArrowKeyInput && _clickContext == ClickContext.Detail)
            {
                int frame = _logDetailSelectedFrame == -2 ? 0 : _logDetailSelectedFrame + 1;
                bool isFrameOutsideOfRange = frame < firstRenderLogIndex + 1 || frame > lastRenderLogIndex - 2;
                if (isFrameOutsideOfRange && _logMoveDirection == Direction.Up)
                {
                    if (frame == 0)
                    {
                        _logDetailScrollPosition.y = 0f;
                    }
                    else
                    {
                        _logDetailScrollPosition.y = firstLogHeight + (frame - 1) * buttonHeight;
                    }
                }
                else if (isFrameOutsideOfRange && _logMoveDirection == Direction.Down)
                {
                    if (frame == 0)
                    {
                        _logDetailScrollPosition.y = 0f;
                    }
                    else
                    {
                        _logDetailScrollPosition.y = firstLogHeight + frame * buttonHeight - windowHeight;
                    }
                }
            }

            // Logging first message
            if (firstRenderLogIndex == 0)
            {
                var styleBack = GetLogBackStyle(0);
                var styleMessage = BluConsoleSkin.MessageDetailFirstLogStyle;
                var rectButton = new Rect(x: 0, y: buttonY, width: viewWidth, height: firstLogHeight);

                var isSelected = _logDetailSelectedFrame == -2;
                var contentMessage = new GUIContent(GetTruncatedMessage(log.Message));

                DrawBackground(rectButton, styleBack, isSelected);
                if (IsRepaintEvent)
                    styleMessage.Draw(rectButton, contentMessage, false, false, isSelected, false);

                bool messageClicked = IsClicked(rectButton);
                if (messageClicked)
                {
                    _clickContext = ClickContext.Detail;
                    bool isLeftClick = Event.current.button == 0;
                    if (_logDetailSelectedFrame != -2)
                        _logDetailLastTimeClicked = 0.0f;
                    if (isLeftClick && !IsDoubleClickLogDetailButton)
                        _logDetailSelectedFrame = -2;
                    if (!isLeftClick)
                        DrawPopup(Event.current, log);
                    if (isLeftClick && _logDetailSelectedFrame == -2)
                    {
                        if (IsDoubleClickLogDetailButton)
                        {
                            _logDetailLastTimeClicked = 0.0f;
                            JumpToSourceFile(log, 0);
                        }
                        else
                            _logDetailLastTimeClicked = EditorApplication.timeSinceStartup;
                    }
                }

                buttonY += firstLogHeight;
            }

            for (int i = firstRenderLogIndex == 0 ? 0 : firstRenderLogIndex - 1; i + 1 < lastRenderLogIndex; i++)
            {
                var contentMessage = new GUIContent(GetTruncatedMessage(log.StackTrace[i].FrameInformation));

                var styleBack = GetLogBackStyle(0);
                var styleMessage = BluConsoleSkin.MessageDetailCallstackStyle;
                var rectButton = new Rect(x: 0, y: buttonY, width: viewWidth, height: buttonHeight);

                var isSelected = i == _logDetailSelectedFrame;
                DrawBackground(rectButton, styleBack, isSelected);
                if (IsRepaintEvent)
                    styleMessage.Draw(rectButton, contentMessage, false, false, isSelected, false);

                bool messageClicked = IsClicked(rectButton);
                if (messageClicked)
                {
                    _clickContext = ClickContext.Detail;
                    bool isLeftClick = Event.current.button == 0;
                    if (_logDetailSelectedFrame != i)
                        _logDetailLastTimeClicked = 0.0f;
                    if (isLeftClick && !IsDoubleClickLogDetailButton)
                        _logDetailSelectedFrame = i;
                    if (isLeftClick && _logDetailSelectedFrame == i)
                    {
                        if (IsDoubleClickLogDetailButton)
                        {
                            _logDetailLastTimeClicked = 0.0f;
                            JumpToSourceFile(log, i);
                        }
                        else
                            _logDetailLastTimeClicked = EditorApplication.timeSinceStartup;
                    }
                }

                buttonY += buttonHeight;
            }

            GUI.EndScrollView();

            return windowHeight;
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
            _logListSelectedMessage = Mathf.Clamp(_logListSelectedMessage, 0, _qtLogs - 1);
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
                    _logListSelectedMessage += direction == Direction.Up ? -1 : +1;
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

        private bool IsDoubleClickLogListButton
        {
            get
            {
                return (EditorApplication.timeSinceStartup - _logListLastTimeClicked) < 0.3f && !_hasKeyboardArrowKeyInput;
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
        
        private float ResizerHeight { get { return 2.0f; } }
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
