using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluConsole.Core;
using BluConsole.Core.UnityLoggerApi;


namespace BluConsole.Editor
{

    public enum ClickContext
    {
        None = 0,
        List = 1,
        Detail = 2
    }

    public class BluConsoleEditorWindow : EditorWindow, IHasCustomMenu
    {

        readonly int MAX_LOGS = 999;
        readonly int MAX_LENGTH_MESSAGE = 999;
        readonly int MAX_LENGTH_COLLAPSE = 999;

        // Cache Variables
        UnityApiEvents unityApiEvents;
        BluLogSettings settings;
        List<BluLog> cacheLog = new List<BluLog>();
        List<bool> cacheLogComparer = new List<bool>();
        List<string> stackTraceIgnorePrefixs = new List<string>();
        int[] cacheIntArr = new int[100];
        BluLog[] cacheLogArr = new BluLog[100];
        int cacheLogCount;

        // Toolbar Variables
        string[] searchStringPatterns;
        string searchString = "";
        bool isClearOnPlay;
        bool isPauseOnError;
        bool isCollapse;
        bool isShowNormal;
        bool isShowWarning;
        bool isShowError;

        // LogList Variables
        Vector2 _logListScrollPosition;
        int logListSelectedMessage = -1;
        double _logListLastTimeClicked = 0.0;
        int _qtLogs = 0;

        // Resizer Variables
        float _topPanelHeight;
        Rect _cursorChangeRect;
        bool _isResizing = false;

        // LogDetail Variables
        Vector2 _logDetailScrollPosition;
        int logDetailSelectedFrame = -1;
        double _logDetailLastTimeClicked;
        BluLog _selectedLog = null;

        // Filter Variables
        List<bool> toggledFilters = new List<bool>();

        // Keyboard Controll Variables
        bool hadArrowClick;
        int moveDir;
        ClickContext clickContext;


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

        void OnEnable()
        {
            stackTraceIgnorePrefixs = BluUtils.StackTraceIgnorePrefixs;
            settings = BluLogSettings.Instance;
            unityApiEvents = UnityApiEvents.Instance;
            SetDirtyLogs();
        }

        void OnDestroy()
        {
            settings = null;
            unityApiEvents = null;
            Resources.UnloadAsset(titleContent.image);
            Resources.UnloadUnusedAssets();
        }

        void Update()
        {
            unityApiEvents.OnBeforeCompileEvent -= SetDirtyLogs;
            unityApiEvents.OnBeforeCompileEvent += SetDirtyLogs;
            unityApiEvents.OnAfterCompileEvent -= OnAfterCompile;
            unityApiEvents.OnAfterCompileEvent += OnAfterCompile;
            unityApiEvents.OnBeginPlayEvent -= SetDirtyLogs;
            unityApiEvents.OnBeginPlayEvent += SetDirtyLogs;
            unityApiEvents.OnStopPlayEvent -= SetDirtyLogs;
            unityApiEvents.OnStopPlayEvent += SetDirtyLogs;
        }

        void OnGUI()
        {
            InitVariables();

            UpdateLogLine();

            DrawResizer();

            GUILayout.BeginVertical(GUILayout.Height(_topPanelHeight), GUILayout.MinHeight(MinHeightOfTopAndBottom));

            DrawYPos += DrawTopToolbar();
            DrawYPos -= 1f;
            DrawLogList();

            GUILayout.EndVertical();

            GUILayout.Space(ResizerHeight);

            GUILayout.BeginVertical(GUILayout.Height(WindowHeight - _topPanelHeight - ResizerHeight));
            DrawYPos = _topPanelHeight + ResizerHeight;
            DrawLogDetail();

            GUILayout.EndVertical();

            CheckEnterAction();

            Repaint();
        }

        void OnAfterCompile()
        {
            SetDirtyLogs();
            logListSelectedMessage = -1;
            logDetailSelectedFrame = -1;
        }

        #region IHasCustomMenu

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Add Filter"), false, OnAddFilterTabClicked);
        }

        void OnAddFilterTabClicked()
        {
            if (settings == null)
                return;
            Selection.activeObject = settings;
        }

        #endregion IHasCustomMenu

        #region Draw

        float DrawTopToolbar()
        {
            float height = BluConsoleSkin.ToolbarButtonStyle.CalcSize("Clear".GUIContent()).y;
            GUILayout.BeginHorizontal(BluConsoleSkin.ToolbarStyle, GUILayout.Height(height));

            if (GetButtonClamped("Clear".GUIContent(), BluConsoleSkin.ToolbarButtonStyle))
            {
                UnityLoggerServer.Clear();
                SetDirtyLogs();
                logListSelectedMessage = -1;
                logDetailSelectedFrame = -1;
            }

            GUILayout.Space(6.0f);

            bool actualCollapse = UnityLoggerServer.HasFlag(ConsoleWindowFlag.Collapse);
            bool newCollapse = GetToggleClamped(isCollapse, "Collapse".GUIContent(), BluConsoleSkin.ToolbarButtonStyle);
            if (newCollapse != isCollapse)
            {
                SetDirtyLogs();
                UnityLoggerServer.SetFlag(ConsoleWindowFlag.Collapse, newCollapse);
                isCollapse = newCollapse;
            }
            else if (newCollapse != actualCollapse)
            {
                SetDirtyLogs();
                isCollapse = actualCollapse;
            }


            bool actualClearOnPlay = UnityLoggerServer.HasFlag(ConsoleWindowFlag.ClearOnPlay);
            bool newClearOnPlay = GetToggleClamped(isClearOnPlay, "Clear on Play".GUIContent(), BluConsoleSkin.ToolbarButtonStyle);
            if (newClearOnPlay != isClearOnPlay)
            {
                UnityLoggerServer.SetFlag(ConsoleWindowFlag.ClearOnPlay, newClearOnPlay);
                isClearOnPlay = newClearOnPlay;
            }
            else if (newClearOnPlay != actualClearOnPlay)
            {
                isClearOnPlay = actualClearOnPlay;
            }


            bool actualPauseOnError = UnityLoggerServer.HasFlag(ConsoleWindowFlag.ErrorPause);
            bool newPauseOnError = GetToggleClamped(isPauseOnError, "Pause on Error".GUIContent(), BluConsoleSkin.ToolbarButtonStyle);
            if (newPauseOnError != isPauseOnError)
            {
                UnityLoggerServer.SetFlag(ConsoleWindowFlag.ErrorPause, newPauseOnError);
                isPauseOnError = newPauseOnError;
            }
            else if (newPauseOnError != actualPauseOnError)
            {
                isPauseOnError = actualPauseOnError;
            }


            GUILayout.FlexibleSpace();

            // Search Area
            var oldString = searchString;
            searchString = EditorGUILayout.TextArea(searchString,
                                                     BluConsoleSkin.ToolbarSearchTextFieldStyle,
                                                     GUILayout.Width(200.0f));
            if (searchString != oldString)
                SetDirtyComparer();

            if (GUILayout.Button("", BluConsoleSkin.ToolbarSearchCancelButtonStyle))
            {
                searchString = "";
                SetDirtyComparer();
                GUI.FocusControl(null);
            }

            searchStringPatterns = searchString.Trim().ToLower().Split(' ');

            GUILayout.Space(10.0f);


            // Info/Warning/Error buttons Area
            int qtNormalLogs = 0, qtWarningLogs = 0, qtErrorLogs = 0;
            UnityLoggerServer.GetCount(ref qtNormalLogs, ref qtWarningLogs, ref qtErrorLogs);

            string qtNormalLogsStr = qtNormalLogs.ToString();
            if (qtNormalLogs >= MAX_LOGS)
                qtNormalLogsStr = MAX_LOGS.ToString() + "+";

            string qtWarningLogsStr = qtWarningLogs.ToString();
            if (qtWarningLogs >= MAX_LOGS)
                qtWarningLogsStr = MAX_LOGS.ToString() + "+";

            string qtErrorLogsStr = qtErrorLogs.ToString();
            if (qtErrorLogs >= MAX_LOGS)
                qtErrorLogsStr = MAX_LOGS.ToString() + "+";


            bool actualIsShowNormal = UnityLoggerServer.HasFlag(ConsoleWindowFlag.LogLevelLog);
            bool newIsShowNormal = GetToggleClamped(isShowNormal,
                                                    new GUIContent(qtNormalLogsStr, BluConsoleSkin.InfoIconSmall),
                                                    BluConsoleSkin.ToolbarButtonStyle);
            if (newIsShowNormal != isShowNormal)
            {
                SetDirtyLogs();
                UnityLoggerServer.SetFlag(ConsoleWindowFlag.LogLevelLog, newIsShowNormal);
                isShowNormal = newIsShowNormal;
            }
            else if (newIsShowNormal != actualIsShowNormal)
            {
                SetDirtyLogs();
                isShowNormal = actualIsShowNormal;
            }


            bool actualIsShowWarning = UnityLoggerServer.HasFlag(ConsoleWindowFlag.LogLevelWarning);
            bool newIsShowWarning = GetToggleClamped(isShowWarning,
                                                     new GUIContent(qtWarningLogsStr, BluConsoleSkin.WarningIconSmall),
                                                     BluConsoleSkin.ToolbarButtonStyle);
            if (newIsShowWarning != isShowWarning)
            {
                SetDirtyLogs();
                UnityLoggerServer.SetFlag(ConsoleWindowFlag.LogLevelWarning, newIsShowWarning);
                isShowWarning = newIsShowWarning;
            }
            else if (newIsShowWarning != actualIsShowWarning)
            {
                SetDirtyLogs();
                isShowWarning = actualIsShowWarning;
            }


            bool actualIsShowError = UnityLoggerServer.HasFlag(ConsoleWindowFlag.LogLevelError);
            bool newIsShowError = GetToggleClamped(isShowError,
                                                   new GUIContent(qtErrorLogsStr, BluConsoleSkin.ErrorIconSmall),
                                                   BluConsoleSkin.ToolbarButtonStyle);
            if (newIsShowError != isShowError)
            {
                SetDirtyLogs();
                UnityLoggerServer.SetFlag(ConsoleWindowFlag.LogLevelError, newIsShowError);
                isShowError = newIsShowError;
            }
            else if (newIsShowError != actualIsShowError)
            {
                SetDirtyLogs();
                isShowError = actualIsShowError;
            }

            for (int i = 0; i < settings.Filters.Count; i++)
            {
                var name = settings.Filters[i].Name;
                var style = BluConsoleSkin.ToolbarButtonStyle;
                bool oldAdditionalFilter = toggledFilters[i];
                toggledFilters[i] = GUILayout.Toggle(toggledFilters[i],
                                                     name,
                                                     style,
                                                     GUILayout.MaxWidth(style.CalcSize(new GUIContent(name)).x));
                if (oldAdditionalFilter != toggledFilters[i])
                    SetDirtyLogs();
            }

            GUILayout.EndHorizontal();

            return height;
        }

        void DrawLogList()
        {
            _qtLogs = UnityLoggerServer.StartGettingLogs();
            cacheLogCount = _qtLogs;

            int cntLogs = 0;
            int[] rows = GetCachedIntArr(_qtLogs);
            BluLog[] logs = GetCachedLogsArr(_qtLogs);
            int index = 0;
            int cacheLogComparerCount = cacheLogComparer.Count;
            for (int i = 0; i < _qtLogs; i++)
            {
                // Ugly code to avoid function call 
                int realCount = cacheLog.Count;
                BluLog log = null;
                if (i < cacheLogCount && i < realCount)
                    log = cacheLog[i];
                else
                    log = GetSimpleLog(i);

                // Ugly code to avoid function call 
                bool has = false;
                if (i < cacheLogComparerCount)
                    has = cacheLogComparer[i];
                else
                    has = HasPattern(log, i);
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

            float buttonWidth = ButtonWidth;
            if (_qtLogs * ButtonHeight > windowHeight)
                buttonWidth -= 15f;

            float viewWidth = buttonWidth;
            float viewHeight = _qtLogs * ButtonHeight;

            Rect scrollViewPosition = new Rect(x: 0f, y: DrawYPos, width: windowWidth, height: windowHeight);
            Rect scrollViewViewRect = new Rect(x: 0f, y: 0f, width: viewWidth, height: viewHeight);

            GUI.DrawTexture(scrollViewPosition, BluConsoleSkin.EvenBackTexture);

            Vector2 oldScrollPosition = _logListScrollPosition;
            _logListScrollPosition = GUI.BeginScrollView(position: scrollViewPosition,
                                                         scrollPosition: _logListScrollPosition,
                                                         viewRect: scrollViewViewRect);

            int firstRenderLogIndex = (int)(_logListScrollPosition.y / ButtonHeight);
            firstRenderLogIndex = Mathf.Clamp(firstRenderLogIndex, 0, _qtLogs);

            int lastRenderLogIndex = firstRenderLogIndex + (int)(windowHeight / ButtonHeight) + 2;
            lastRenderLogIndex = Mathf.Clamp(lastRenderLogIndex, 0, _qtLogs);

            if (hadArrowClick && clickContext == ClickContext.List)
            {
                bool isFrameOutsideOfRange = logListSelectedMessage < firstRenderLogIndex + 1 ||
                                             logListSelectedMessage > lastRenderLogIndex - 3;
                if (isFrameOutsideOfRange && moveDir == 1)
                {
                    _logListScrollPosition.y = ButtonHeight * logListSelectedMessage;
                }
                else if (isFrameOutsideOfRange && moveDir == -1)
                {
                    int md = lastRenderLogIndex - firstRenderLogIndex - 3;
                    float ss = md * ButtonHeight;
                    float sd = windowHeight - ss;
                    _logListScrollPosition.y = (ButtonHeight * (logListSelectedMessage + 1) - ss - sd);
                }
            }

            float buttonY = firstRenderLogIndex * ButtonHeight;
            bool hasSomeClick = false;

            int cnt = 0;
            bool hasCollapse = UnityLoggerServer.HasFlag(ConsoleWindowFlag.Collapse);
            for (int i = firstRenderLogIndex; i < lastRenderLogIndex; i++)
            {
                var row = rows[i];
                var log = logs[i];
                var styleBack = GetLogBackStyle(i);

                var styleMessage = BluConsoleSkin.GetLogListStyle(log.LogType);
                string showMessage = GetTruncatedMessage(GetLogListMessage(log));
                var contentMessage = new GUIContent(showMessage);
                var rectMessage = new Rect(x: 0,
                                           y: buttonY,
                                           width: viewWidth,
                                           height: ButtonHeight);
                bool isSelected = i == logListSelectedMessage ? true : false;
                DrawBack(rectMessage, styleBack, isSelected);
                if (IsRepaintEvent)
                    styleMessage.Draw(rectMessage, contentMessage, false, false, isSelected, false);

                bool messageClicked = IsClicked(rectMessage);
                bool isLeftClick = messageClicked ? Event.current.button == 0 : false;

                if (hasCollapse)
                {
                    int quantity = UnityLoggerServer.GetLogCount(row);
                    var collapseCount = Mathf.Min(quantity, MAX_LENGTH_COLLAPSE);
                    var collapseText = collapseCount.ToString();
                    if (collapseCount >= MAX_LENGTH_COLLAPSE)
                        collapseText += "+";
                    var collapseContent = new GUIContent(collapseText);
                    var collapseSize = BluConsoleSkin.CollapseStyle.CalcSize(collapseContent);

                    var collapseRect = new Rect(x: viewWidth - collapseSize.x - 5f,
                                                y: (buttonY + buttonY + ButtonHeight - collapseSize.y) * 0.5f,
                                                width: collapseSize.x,
                                                height: collapseSize.y);

                    GUI.Label(collapseRect, collapseContent, BluConsoleSkin.CollapseStyle);
                }

                if (messageClicked)
                {
                    clickContext = ClickContext.List;

                    _selectedLog = GetCompleteLog(row);

                    hasSomeClick = true;

                    if (!isLeftClick && i == logListSelectedMessage)
                        DrawPopup(Event.current, log);

                    if (isLeftClick && i == logListSelectedMessage)
                    {
                        if (IsDoubleClickLogListButton)
                        {
                            _logListLastTimeClicked = 0.0f;
                            var completeLog = GetCompleteLog(row);
                            JumpToSource(completeLog, 0);
                        }
                        else
                        {
                            PingLog(_selectedLog);
                            _logListLastTimeClicked = EditorApplication.timeSinceStartup;
                        }
                    }
                    else
                    {
                        PingLog(_selectedLog);
                        logListSelectedMessage = i;
                    }

                    logDetailSelectedFrame = -1;
                }

                buttonY += ButtonHeight;
                cnt++;
            }

            UnityLoggerServer.StopGettingsLogs();

            GUI.EndScrollView();

            if (HasScrollUp || hasSomeClick)
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
                return;

            float endY = viewHeight - windowHeight;
            _logListScrollPosition.y = endY;
        }

        void DrawResizer()
        {
            var resizerY = _topPanelHeight;

            _cursorChangeRect = new Rect(0, resizerY - 2f, position.width, ResizerHeight + 3f);
            var cursorChangeCenterRect = new Rect(0, resizerY, position.width, 1.0f);

            if (IsRepaintEvent)
                BluConsoleSkin.BoxStyle.Draw(cursorChangeCenterRect, false, false, false, false);
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

            _topPanelHeight = Mathf.Clamp(_topPanelHeight,
                                          MinHeightOfTopAndBottom,
                                          position.height - MinHeightOfTopAndBottom);
        }

        void DrawLogDetail()
        {
            var windowHeight = WindowHeight - DrawYPos;

            {
                var rect = new Rect(x: 0, y: DrawYPos, width: WindowWidth, height: windowHeight);
                GUI.DrawTexture(rect, BluConsoleSkin.EvenBackTexture);
            }

            if (logListSelectedMessage == -1 ||
                _qtLogs == 0 ||
                logListSelectedMessage >= _qtLogs ||
                _selectedLog == null ||
                _selectedLog.StackTrace == null)
            {
                return;
            }

            var log = _selectedLog;

            var size = log.StackTrace.Count;
            var sizePlus = size + 1;

            float buttonHeight = GetDetailMessageHeight("A", BluConsoleSkin.MessageDetailCallstackStyle);
            float buttonWidth = ButtonWidth;
            float firstLogHeight = Mathf.Max(buttonHeight, GetDetailMessageHeight(GetTruncatedMessage(log.Message),
                                                                                  BluConsoleSkin.MessageDetailFirstLogStyle,
                                                                                  buttonWidth));

            float viewHeight = size * buttonHeight + firstLogHeight;

            if (viewHeight > windowHeight)
                buttonWidth -= 15f;

            // Recalculate it because we decreased the buttonWidth
            firstLogHeight = Mathf.Max(buttonHeight, GetDetailMessageHeight(GetTruncatedMessage(log.Message),
                                                                            BluConsoleSkin.MessageDetailFirstLogStyle,
                                                                            buttonWidth));
            viewHeight = size * buttonHeight + firstLogHeight;

            float viewWidth = buttonWidth;

            Rect scrollViewPosition = new Rect(x: 0f, y: DrawYPos, width: WindowWidth, height: windowHeight);
            Rect scrollViewViewRect = new Rect(x: 0f, y: 0f, width: viewWidth, height: viewHeight);

            _logDetailScrollPosition = GUI.BeginScrollView(position: scrollViewPosition,
                                                           scrollPosition: _logDetailScrollPosition,
                                                           viewRect: scrollViewViewRect);

            // Return if has nothing to show
            if (logListSelectedMessage == -1 || _qtLogs == 0 || logListSelectedMessage >= _qtLogs)
            {
                GUI.EndScrollView();
                return;
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

            if (hadArrowClick && clickContext == ClickContext.Detail)
            {
                int frame = logDetailSelectedFrame == -2 ? 0 : logDetailSelectedFrame + 1;
                bool isFrameOutsideOfRange = frame < firstRenderLogIndex + 1 || frame > lastRenderLogIndex - 2;
                if (isFrameOutsideOfRange && moveDir == 1)
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
                else if (isFrameOutsideOfRange && moveDir == -1)
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

                var isSelected = logDetailSelectedFrame == -2;
                var contentMessage = new GUIContent(GetTruncatedMessage(log.Message));

                DrawBack(rectButton, styleBack, isSelected);
                if (IsRepaintEvent)
                    styleMessage.Draw(rectButton, contentMessage, false, false, isSelected, false);

                bool messageClicked = IsClicked(rectButton);
                if (messageClicked)
                {
                    clickContext = ClickContext.Detail;

                    bool isLeftClick = Event.current.button == 0;

                    if (!isLeftClick && logDetailSelectedFrame == -2)
                        DrawPopup(Event.current, log);

                    if (isLeftClick && logDetailSelectedFrame == -2)
                    {
                        if (IsDoubleClickLogDetailButton)
                        {
                            _logDetailLastTimeClicked = 0.0f;
                            JumpToSource(log, 0);
                        }
                        else
                        {
                            _logDetailLastTimeClicked = EditorApplication.timeSinceStartup;
                        }
                    }
                    else
                    {
                        logDetailSelectedFrame = -2;
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

                var isSelected = i == logDetailSelectedFrame;
                DrawBack(rectButton, styleBack, isSelected);
                if (IsRepaintEvent)
                    styleMessage.Draw(rectButton, contentMessage, false, false, isSelected, false);

                bool messageClicked = IsClicked(rectButton);
                if (messageClicked)
                {
                    clickContext = ClickContext.Detail;

                    bool isLeftClick = Event.current.button == 0;

                    if (isLeftClick && i == logDetailSelectedFrame)
                    {
                        if (IsDoubleClickLogDetailButton)
                        {
                            _logDetailLastTimeClicked = 0.0f;
                            JumpToSource(log, i);
                        }
                        else
                        {
                            _logDetailLastTimeClicked = EditorApplication.timeSinceStartup;
                        }
                    }
                    else
                    {
                        logDetailSelectedFrame = i;
                    }
                }

                buttonY += buttonHeight;
            }

            GUI.EndScrollView();
        }

        void DrawPopup(Event clickEvent, BluLog log)
        {
            GenericMenu.MenuFunction copyCallback = () => { EditorGUIUtility.systemCopyBuffer = log.Message; };

            GenericMenu menu = new GenericMenu();
            menu.AddItem(content: "Copy".GUIContent(), on: false, func: copyCallback);
            menu.ShowAsContext();

            clickEvent.Use();
        }

        void DrawBack(Rect rect, GUIStyle style, bool isSelected)
        {
            if (IsRepaintEvent)
                style.Draw(rect, false, false, isSelected, false);
        }

        #endregion Draw

        #region Gets

        int[] GetCachedIntArr(int size)
        {
            if (size > cacheIntArr.Length)
                cacheIntArr = new int[size * 2];
            return cacheIntArr;
        }

        BluLog[] GetCachedLogsArr(int size)
        {
            if (size > cacheLogArr.Length)
                cacheLogArr = new BluLog[size * 2];
            return cacheLogArr;
        }

        BluLog GetSimpleLog(int row)
        {
            int realCount = cacheLog.Count;
            var log = UnityLoggerServer.GetSimpleLog(row);
            if (realCount > row)
                cacheLog[row] = log;
            else
                cacheLog.Add(log);
            return cacheLog[row];
        }

        BluLog GetCompleteLog(int row)
        {
            var log = UnityLoggerServer.GetCompleteLog(row);
            log.FilterStackTrace(stackTraceIgnorePrefixs);
            return log;
        }

        string GetTruncatedMessage(string message)
        {
            if (message.Length <= MAX_LENGTH_MESSAGE)
                return message;

            return string.Format("{0}... <truncated>", message.Substring(startIndex: 0, length: MAX_LENGTH_MESSAGE));
        }

        string GetLogListMessage(BluLog log)
        {
            return log.Message.Replace(System.Environment.NewLine, " ");
        }

        GUIStyle GetLogBackStyle(int row)
        {
            return row % 2 == 0 ? BluConsoleSkin.EvenBackStyle : BluConsoleSkin.OddBackStyle;
        }

        float GetDetailMessageHeight(string message, GUIStyle style, float width = 0f)
        {
            return style.CalcHeight(new GUIContent(message), width);
        }

        bool GetButtonClamped(GUIContent content, GUIStyle style)
        {
            return GUILayout.Button(content, style, GUILayout.MaxWidth(style.CalcSize(new GUIContent(content)).x));
        }

        bool GetToggleClamped(bool state, GUIContent content, GUIStyle style)
        {
            return GUILayout.Toggle(state, content, style, GUILayout.MaxWidth(style.CalcSize(content).x));
        }

        bool IsClicked(Rect rect)
        {
            return Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition);
        }

        bool HasPattern(BluLog log, int row)
        {
            string messageLower = log.MessageLower;

            int size = searchStringPatterns.Length;
            for (int i = 0; i < size; i++)
            {
                string pattern = searchStringPatterns[i];
                if (pattern == "")
                    continue;

                if (!messageLower.Contains(pattern))
                {
                    CacheLogComparer(row, false);
                    return false;
                }
            }

            bool hasFilter = false;
            bool hasPattern = false;
            for (int i = 0; i < settings.Filters.Count; i++)
            {
                if (!toggledFilters[i])
                    continue;

                hasFilter = true;
                foreach (var pattern in settings.Filters[i].Patterns)
                {
                    if (messageLower.Contains(pattern))
                    {
                        hasPattern = true;
                        break;
                    }
                }
            }

            hasPattern |= !hasFilter;
            CacheLogComparer(row, hasPattern);
            return hasPattern;
        }

        #endregion Gets

        #region Action

        void CheckEnterAction()
        {
            if (!IsEnterPressed || _selectedLog == null)
                return;

            if (clickContext == ClickContext.List)
                JumpToSource(_selectedLog, 0);
            else if (clickContext == ClickContext.Detail)
                JumpToSource(_selectedLog, logDetailSelectedFrame < 0 ? 0 : logDetailSelectedFrame);
        }

        void UpdateLogLine()
        {
            // Handles moving up and down using the arrow keys on the keyboard.
            Event e = Event.current;
            hadArrowClick = false;
            moveDir = 0;
            if (e != null && e.type == EventType.KeyDown && e.isKey)
            {
                bool refresh = false;
                switch (e.keyCode)
                {
                    case KeyCode.UpArrow:
                        refresh = true;
                        moveDir = 1;
                        if (clickContext == ClickContext.List)
                        {
                            logListSelectedMessage--;
                        }
                        else
                        {
                            if (logDetailSelectedFrame == 0)
                                logDetailSelectedFrame = -2;
                            else
                                logDetailSelectedFrame--;
                        }
                        break;
                    case KeyCode.DownArrow:
                        refresh = true;
                        moveDir = -1;
                        if (clickContext == ClickContext.List)
                        {
                            logListSelectedMessage++;
                        }
                        else
                        {
                            if (logDetailSelectedFrame == -2)
                                logDetailSelectedFrame = 0;
                            else
                                logDetailSelectedFrame++;
                        }
                        break;
                }

                if (!refresh)
                    return;

                hadArrowClick = true;
                logListSelectedMessage = Mathf.Clamp(logListSelectedMessage, 0, _qtLogs - 1);
                if (_selectedLog != null)
                {
                    if (logDetailSelectedFrame < -2)
                        logDetailSelectedFrame = -2;
                    else if (logDetailSelectedFrame >= _selectedLog.StackTrace.Count)
                        logDetailSelectedFrame = _selectedLog.StackTrace.Count - 1;
                }
            }
        }

        void PingLog(BluLog log)
        {
            if (log.InstanceID != 0)
                EditorGUIUtility.PingObject(log.InstanceID);
        }

        void CacheLogComparer(int row, bool value)
        {
            if (row < cacheLogComparer.Count)
                cacheLogComparer[row] = value;
            else
                cacheLogComparer.Add(value);
        }

        void JumpToSource(BluLog log, int row)
        {
            var file = "";
            var line = -1;
            var frames = log.StackTrace;

            if (frames.Count == 0)
            {
                file = log.File;
                line = log.Line;
            }
            else if (row < frames.Count)
            {
                file = frames[row].File;
                line = frames[row].Line;
            }

            if (string.IsNullOrEmpty(file) || line == -1)
                return;

            BluUtils.OpenFileOnEditor(file, line);
        }

        void SetDirtyComparer()
        {
            cacheLogComparer.Clear();
        }

        void SetDirtyLogs()
        {
            cacheLog.Clear();
            cacheLogCount = 0;
            SetDirtyComparer();
        }

        void InitVariables()
        {
            while (toggledFilters.Count < settings.Filters.Count)
                toggledFilters.Add(false);
            ButtonWidth = position.width;
            ButtonHeight = BluConsoleSkin.MessageStyle.CalcSize("Test".GUIContent()).y + 15.0f;
            DrawYPos = 0f;
        }

        #endregion

        #region Properties

        private bool IsEnterPressed
        {
            get
            {
                Event e = Event.current;
                return e != null && e.isKey && e.type == EventType.KeyUp && e.keyCode == KeyCode.Return;
            }
        }

        bool IsFollowScroll
        {
            get;
            set;
        }

        bool HasScrollUp
        {
            get
            {
                return Event.current.type == EventType.ScrollWheel && Event.current.delta.y < 0f;
            }
        }

        bool IsRepaintEvent
        {
            get
            {
                return Event.current.type == EventType.Repaint;
            }
        }

        bool IsDoubleClickLogListButton
        {
            get
            {
                return (EditorApplication.timeSinceStartup - _logListLastTimeClicked) < 0.3f && !hadArrowClick;
            }
        }

        bool IsDoubleClickLogDetailButton
        {
            get
            {
                return (EditorApplication.timeSinceStartup - _logDetailLastTimeClicked) < 0.3f && !hadArrowClick;
            }
        }

        float DrawYPos
        {
            get;
            set;
        }

        float WindowWidth
        {
            get
            {
                return position.width;
            }
        }

        float WindowHeight
        {
            get
            {
                return position.height;
            }
        }

        float ButtonWidth
        {
            get;
            set;
        }

        float ButtonHeight
        {
            get;
            set;
        }

        float ResizerHeight
        {
            get
            {
                return 1.0f;
            }
        }

        float MinHeightOfTopAndBottom
        {
            get
            {
                return 60.0f;
            }
        }

        #endregion Properties

    }

}
