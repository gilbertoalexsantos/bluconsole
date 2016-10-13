#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


namespace BluConsole.Editor
{

public class BluConsoleEditorWindow : EditorWindow
{
    // LoggerClient
    private LoggerAssetClient _loggerAsset;

    // Cache Variables
    private GUIStyle _evenButtonStyle;
    private GUIStyle _oddButtonStyle;
    private GUIStyle _evenErrorButtonStyle;
    private GUIStyle _oddButtonErrorStyle;
    private GUIStyle _selectedButtonStyle;
    private float _buttonWidth;
    private float _buttonHeight;

    // Toolbar Variables
    private float _toolbarHeight;
    private bool _isShowNormal = true;
    private bool _isShowWarnings = true;
    private bool _isShowErrors = true;
    private bool _isClearOnPlay = false;
    private string _searchString = "";

    // LogList Variables
    private Vector2 _logListBeginPosition;
    private int _logListSelectedMessage = -1;
    private double _logListLastTimeClicked = 0.0;

    // Resizer
    private float _topPanelHeight;
    private Rect _cursorChangeRect;
    private bool _isResizing = false;

    // LogDetail Variables
    private Vector2 _logDetailBeginPosition;
    private int _logDetailSelectedFrame = -1;
    private double _logDetailLastTimeClicked = 0.0;

    // Compiling
    private bool _isCompiling = false;
    private bool _isPlaying = false;
    private List<LogInfo> _dirtyLogsBeforeCompile = new List<LogInfo>();

    [MenuItem("Window/BluConsole")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow<BluConsoleEditorWindow>("BluConsole");
        window._topPanelHeight = window.position.height / 2.0f;
    }

    private void OnGUI()
    {
        InitVariables();

        Application.logMessageReceived -= LoggerServer.UnityLogHandler;
        Application.logMessageReceived += LoggerServer.UnityLogHandler;

        if (EditorApplication.isCompiling && !_isCompiling) {
            _isCompiling = true;
            OnBeforeCompile();
        } else if (!EditorApplication.isCompiling && _isCompiling) {
            _isCompiling = false;
            OnAfterCompile();
        }

        if (EditorApplication.isPlaying && !_isPlaying) {
            _isPlaying = true;
            OnBeginPlay();
        } else if (!EditorApplication.isPlaying && _isPlaying) {
            _isPlaying = false;
        }

        DrawResizer();

        GUILayout.BeginVertical(GUILayout.Height(_topPanelHeight), GUILayout.MinHeight(MinHeightOfTopAndBottom));
        DrawTopToolbar();
        DrawLogList();
        GUILayout.EndVertical();

        GUILayout.Space(ResizerHeight);

        GUILayout.BeginVertical(GUILayout.Height(WindowHeight - _topPanelHeight - ResizerHeight));
        DrawLogDetail();
        GUILayout.EndVertical();

        Repaint();
    }

    private void InitVariables()
    {
        _evenButtonStyle = new GUIStyle(EditorStyles.label);
        _evenButtonStyle.richText = true;
        _evenButtonStyle.fontSize = FontSize;
        _evenButtonStyle.alignment = TextAnchor.MiddleLeft;
        _evenButtonStyle.margin = new RectOffset(0, 0, 0, 0);
        _evenButtonStyle.padding = new RectOffset(7, 0, 0, 0);
        _evenButtonStyle.normal.background = EvenButtonTexture;
        _evenButtonStyle.active = _evenButtonStyle.normal;
        _evenButtonStyle.hover = _evenButtonStyle.normal;
        _evenButtonStyle.focused = _evenButtonStyle.normal;

        _oddButtonStyle = new GUIStyle(EditorStyles.label);
        _oddButtonStyle.richText = true;
        _oddButtonStyle.fontSize = FontSize;
        _oddButtonStyle.alignment = TextAnchor.MiddleLeft;
        _oddButtonStyle.margin = new RectOffset(0, 0, 0, 0);
        _oddButtonStyle.padding = new RectOffset(7, 0, 0, 0);
        _oddButtonStyle.normal.background = OddButtonTexture;
        _oddButtonStyle.active = _oddButtonStyle.normal;
        _oddButtonStyle.hover = _oddButtonStyle.normal;
        _oddButtonStyle.focused = _oddButtonStyle.normal;

        _evenErrorButtonStyle = new GUIStyle(EditorStyles.label);
        _evenErrorButtonStyle.richText = true;
        _evenErrorButtonStyle.fontSize = FontSize;
        _evenErrorButtonStyle.alignment = TextAnchor.MiddleLeft;
        _evenErrorButtonStyle.margin = new RectOffset(0, 0, 0, 0);
        _evenErrorButtonStyle.padding = new RectOffset(7, 0, 0, 0);
        _evenErrorButtonStyle.normal.background = EvenErrorButtonTexture;
        _evenErrorButtonStyle.active = _evenErrorButtonStyle.normal;
        _evenErrorButtonStyle.hover = _evenErrorButtonStyle.normal;
        _evenErrorButtonStyle.focused = _evenErrorButtonStyle.normal;

        _oddButtonErrorStyle = new GUIStyle(EditorStyles.label);
        _oddButtonErrorStyle.richText = true;
        _oddButtonErrorStyle.fontSize = FontSize;
        _oddButtonErrorStyle.alignment = TextAnchor.MiddleLeft;
        _oddButtonErrorStyle.margin = new RectOffset(0, 0, 0, 0);
        _oddButtonErrorStyle.padding = new RectOffset(7, 0, 0, 0);
        _oddButtonErrorStyle.normal.background = OddErrorButtonTexture;
        _oddButtonErrorStyle.active = _oddButtonErrorStyle.normal;
        _oddButtonErrorStyle.hover = _oddButtonErrorStyle.normal;
        _oddButtonErrorStyle.focused = _oddButtonErrorStyle.normal;

        _selectedButtonStyle = new GUIStyle(EditorStyles.whiteLabel);
        _selectedButtonStyle.richText = true;
        _selectedButtonStyle.fontSize = FontSize;
        _selectedButtonStyle.alignment = TextAnchor.MiddleLeft;
        _selectedButtonStyle.margin = new RectOffset(0, 0, 0, 0);
        _selectedButtonStyle.padding = new RectOffset(7, 0, 0, 0);
        _selectedButtonStyle.normal.background = BluConsoleEditorHelper.BlueTexture;
        _selectedButtonStyle.active = _selectedButtonStyle.normal;
        _selectedButtonStyle.hover = _selectedButtonStyle.normal;
        _selectedButtonStyle.focused = _selectedButtonStyle.normal;

        _buttonWidth = position.width;
        _buttonHeight = _evenButtonStyle.CalcSize(new GUIContent("Test")).y + 15.0f;
        _toolbarHeight = EditorStyles.toolbarButton.CalcSize(new GUIContent("Clear")).y;
    }

    private void OnBeforeCompile()
    {
        _dirtyLogsBeforeCompile = new List<LogInfo>(_loggerAsset.LogsInfo.Where(log => log.IsCompileMessage));
    }

    private void OnAfterCompile()
    {
        HashSet<LogInfo> logsBlackList = new HashSet<LogInfo>(_dirtyLogsBeforeCompile,
                                                              new LogInfoComparer());
        _loggerAsset.Clear(log => logsBlackList.Contains(log));
    }

    private void OnBeginPlay()
    {
        if (_isClearOnPlay)
            _loggerAsset.Clear();
    }

    private void OnEnable()
    {
        if (_loggerAsset == null) {
            _loggerAsset = LoggerServer.GetLoggerClient<LoggerAssetClient>() as LoggerAssetClient;
            if (_loggerAsset == null)
                _loggerAsset = LoggerAssetClient.GetOrCreate();
        }

        _logListSelectedMessage = -1;
        _logDetailSelectedFrame = -1;

        LoggerServer.Register(_loggerAsset);
    }

    private void DrawTopToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);


        // Clear/Collapse/ClearOnPlay/ErrorPause Area
        if (BluConsoleEditorHelper.ButtonClamped("Clear", EditorStyles.toolbarButton)) {
            _loggerAsset.ClearExceptCompileErrors();
            _logListSelectedMessage = -1;
            _logDetailSelectedFrame = -1;
        }

        GUILayout.Space(6.0f);

        _isClearOnPlay = BluConsoleEditorHelper.ToggleClamped(_isClearOnPlay,
                                                              "Clear on Play",
                                                              EditorStyles.toolbarButton);
        _loggerAsset.IsPauseOnError = BluConsoleEditorHelper.ToggleClamped(_loggerAsset.IsPauseOnError,
                                                                           "Pause on Error",
                                                                           EditorStyles.toolbarButton);


        GUILayout.FlexibleSpace();


        // Search Area
        _searchString = EditorGUILayout.TextArea(_searchString,
                                                 BluConsoleEditorHelper.ToolbarSearchTextField,
                                                 GUILayout.Width(200.0f));
        if (GUILayout.Button("", BluConsoleEditorHelper.ToolbarSearchCancelButtonStyle)) {
            _searchString = "";
            GUI.FocusControl(null);
        }


        GUILayout.Space(10.0f);


        // Info/Warning/Error buttons Area
        int maxLogs = LoggerAssetClient.MAX_LOGS;
        string qtNormalLogs = _loggerAsset.QtNormalLogs.ToString();
        if (_loggerAsset.QtNormalLogs > maxLogs)
            qtNormalLogs = maxLogs.ToString() + "+";

        string qtWarningLogs = _loggerAsset.QtWarningsLogs.ToString();
        if (_loggerAsset.QtWarningsLogs > maxLogs)
            qtWarningLogs = maxLogs.ToString() + "+";

        string qtErrorLogs = _loggerAsset.QtErrorsLogs.ToString();
        if (_loggerAsset.QtErrorsLogs > LoggerAssetClient.MAX_LOGS)
            qtErrorLogs = maxLogs.ToString() + "+";
        
        _isShowNormal =
            BluConsoleEditorHelper.ToggleClamped(_isShowNormal,
                                                 BluConsoleEditorHelper.InfoGUIContent(qtNormalLogs),
                                                 EditorStyles.toolbarButton);
        _isShowWarnings =
            BluConsoleEditorHelper.ToggleClamped(_isShowWarnings,
                                                 BluConsoleEditorHelper.WarningGUIContent(qtWarningLogs),
                                                 EditorStyles.toolbarButton);
        _isShowErrors =
            BluConsoleEditorHelper.ToggleClamped(_isShowErrors,
                                                 BluConsoleEditorHelper.ErrorGUIContent(qtErrorLogs),
                                                 EditorStyles.toolbarButton);


        GUILayout.EndHorizontal();
    }

    private void DrawLogList()
    {
        _logListBeginPosition = GUILayout.BeginScrollView(_logListBeginPosition, false, false);

        var logListHeight = _topPanelHeight - _toolbarHeight;
        var buttonY = 0.0f;

        // Filtering by SearchString
        var logsInfo =
            string.IsNullOrEmpty(_searchString) ? _loggerAsset.LogsInfo : _loggerAsset.GetLogsInfoFiltered(_searchString);

        LogInfo[] logsToShow = new LogInfo[logsInfo.Count];
        int size = 0;
        for (int i = 0, j = 0; i < logsInfo.Count; i++) {
            if (!CanLog(logsInfo[i]))
                continue;
            size++;
            logsToShow[j++] = logsInfo[i];
        }
            
        int qtToLog = Mathf.CeilToInt(logListHeight / ButtonHeight) + 1;
        qtToLog = Mathf.Clamp(qtToLog, 0, size);

        int qtTopEmptySpace = (int)(_logListBeginPosition.y / ButtonHeight);
        qtTopEmptySpace = Mathf.Clamp(qtTopEmptySpace, 0, Mathf.Max(0, size - qtToLog));

        int qtBotEmptySpace = Mathf.Max(0, size - qtToLog - qtTopEmptySpace);
        int qtFillCenterLog = size < qtToLog ? qtToLog - size : 0;

        GUILayout.Space(qtTopEmptySpace * ButtonHeight);

        for (int i = qtTopEmptySpace, j = 0; i < size && j < qtToLog; i++, j++) {
            LogInfo logInfo = logsToShow[i];
            bool isEven = i % 2 == 0;

            string showMessage = null;
            if (logInfo.IsCompileMessage) {
                showMessage = logInfo.RawMessage;
            } else {
                showMessage = logInfo.Message.Replace(System.Environment.NewLine, " ");
            }
            showMessage = "  " + showMessage;

            var content = new GUIContent(showMessage, GetIcon(logInfo.LogType));

            var actualLogLineStyle = EvenButtonStyle;
            if (logInfo.IsCompileMessage && logInfo.LogType == BluLogType.Error && i != _logListSelectedMessage) {
                actualLogLineStyle = isEven ? EvenErrorButtonStyle : OddButtonErrorStyle;
            } else {
                actualLogLineStyle = i == _logListSelectedMessage ?
                    SelectedButtonStyle : (isEven ? EvenButtonStyle : OddButtonStyle);
            }

            if (GUILayout.Button(content,
                                 actualLogLineStyle, 
                                 GUILayout.Height(ButtonHeight),
                                 GUILayout.Width(ButtonWidth))) {
                if (i == _logListSelectedMessage) {
                    if (IsDoubleClickLogListButton()) {
                        _logListLastTimeClicked = 0.0f;
                        if (logInfo.CallStack.Count > 0) {
                            JumpToSource(logInfo.CallStack[0]);
                        }
                    } else {
                        _logListLastTimeClicked = EditorApplication.timeSinceStartup;
                    }
                } else {
                    _logListSelectedMessage = i;
                }

                _logDetailSelectedFrame = -1;
            }

            buttonY += ButtonHeight;
        }

        if (qtFillCenterLog > 0) {
            GUI.DrawTexture(new Rect(_logListBeginPosition.x, _logListBeginPosition.y + size * ButtonHeight, 
                                     position.width, qtFillCenterLog * ButtonHeight), 
                            EvenButtonTexture);
        }

        GUILayout.Space(qtBotEmptySpace * ButtonHeight);

        GUILayout.EndScrollView();
    }

    private void DrawResizer()
    {
        // Don't ask me why... If we remove this 1.0f, there's one line with height one that isn't painted...
        var resizerY = _topPanelHeight - 1.0f;

        _cursorChangeRect = new Rect(0, resizerY, position.width, ResizerHeight);

        var cursorChangeBorderTopRect = new Rect(0, resizerY, position.width, 1.0f);
        var cursorChangeCenterRect = new Rect(0, resizerY + 1.0f, position.width, 2.0f);
        var cursorChangeBorderBottomRect = new Rect(0, resizerY + 3.0f, position.width, 1.0f);

        GUI.DrawTexture(cursorChangeBorderTopRect, SizeLinerBorderTexture);
        GUI.DrawTexture(cursorChangeCenterRect, SizeLinerCenterTexture);
        GUI.DrawTexture(cursorChangeBorderBottomRect, SizeLinerBorderTexture);
        EditorGUIUtility.AddCursorRect(_cursorChangeRect, MouseCursor.ResizeVertical);

        if (Event.current.type == EventType.MouseDown && _cursorChangeRect.Contains(Event.current.mousePosition))
            _isResizing = true;
        else if (Event.current.rawType == EventType.MouseUp)
            _isResizing = false;

        if (_isResizing) {
            _topPanelHeight = Event.current.mousePosition.y;
            _cursorChangeRect.Set(_cursorChangeRect.x, resizerY, _cursorChangeRect.width, _cursorChangeRect.height);
        }

        _topPanelHeight = Mathf.Clamp(_topPanelHeight, MinHeightOfTopAndBottom, position.height - MinHeightOfTopAndBottom);
    }

    private void DrawLogDetail()
    {
        _logDetailBeginPosition = GUILayout.BeginScrollView(_logDetailBeginPosition);

        if (_logListSelectedMessage == -1 ||
            _loggerAsset.QtLogs == 0 ||
            _loggerAsset.QtLogs < _logListSelectedMessage + 1) {
            GUI.DrawTexture(new Rect(_logDetailBeginPosition.x, _logDetailBeginPosition.y, 
                                     position.width, position.height), 
                            EvenButtonTexture);
            GUILayout.EndScrollView();
            return;
        }

        var logDetailHeight = WindowHeight;
        var buttonHeight = ButtonHeight;

        var log = _loggerAsset[_logListSelectedMessage];
        var size = log.CallStack.Count;

        int qtToLog = (int)Mathf.Floor(logDetailHeight / buttonHeight);
        int qtTopEmptySpace = (int)(_logDetailBeginPosition.y / buttonHeight);
        qtTopEmptySpace = Mathf.Clamp(qtTopEmptySpace, 0, Mathf.Max(0, size - qtToLog));
        int qtBotEmptySpace = Mathf.Max(0, size - qtToLog - qtTopEmptySpace);
        int qtFillCenterLog = size < qtToLog ? qtToLog - size : 0;

        GUILayout.Space(qtTopEmptySpace * buttonHeight);

        for (int i = qtTopEmptySpace, j = 0; i < size && j < qtToLog; i++, j++) {
            var frame = log.CallStack[i];
            var methodName = frame.FormattedMethodName;

            if (log.IsCompileMessage)
                methodName = log.RawMessage;

            var actualLogLineStyle = i == _logDetailSelectedFrame ?
                SelectedButtonStyle : (i % 2 == 0 ? EvenButtonStyle : OddButtonStyle);

            if (GUILayout.Button(methodName, actualLogLineStyle, GUILayout.Height(buttonHeight))) {
                if (i == _logDetailSelectedFrame) {
                    if (IsDoubleClickLogDetailButton()) {
                        _logDetailLastTimeClicked = 0.0f;
                        JumpToSource(frame);
                    } else {
                        _logDetailLastTimeClicked = EditorApplication.timeSinceStartup;
                    }
                } else {
                    _logDetailSelectedFrame = i;
                }
            }
        }

        if (qtFillCenterLog > 0) {
            GUI.DrawTexture(new Rect(_logDetailBeginPosition.x, _logDetailBeginPosition.y + size * buttonHeight, 
                                     position.width, qtFillCenterLog * buttonHeight), 
                            EvenButtonTexture);
        }

        GUILayout.Space(qtBotEmptySpace * buttonHeight);

        GUILayout.EndScrollView();
    }

    private Texture2D GetIcon(BluLogType logType)
    {
        switch (logType) {
        case BluLogType.Normal:
            return BluConsoleEditorHelper.InfoIcon;
        case BluLogType.Warning:
            return BluConsoleEditorHelper.WarningIcon;
        case BluLogType.Error:
            return BluConsoleEditorHelper.ErrorIcon;
        default:
            return BluConsoleEditorHelper.InfoIcon;
        }
    }

    private bool CanLog(LogInfo log)
    {
        return (((int)log.LogType) & LogTypeMask) != 0;
    }

    private bool IsDoubleClickLogListButton()
    {
        return (EditorApplication.timeSinceStartup - _logListLastTimeClicked) < 0.3f;
    }

    private bool IsDoubleClickLogDetailButton()
    {
        return (EditorApplication.timeSinceStartup - _logDetailLastTimeClicked) < 0.3f;
    }

    private void JumpToSource(LogStackFrame frame)
    {
        var filename = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), frame.FileRelativePath);
        if (System.IO.File.Exists(filename)) {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(frame.FileRelativePath, frame.Line);
        }
    }

    private int LogTypeMask
    {
        get
        {
            int mask = 0;
            if (_isShowNormal)
                mask |= (int)BluLogType.Normal;
            if (_isShowWarnings)
                mask |= (int)BluLogType.Warning;
            if (_isShowErrors)
                mask |= (int)BluLogType.Error;
            return mask;
        }
    }

    private float WindowHeight
    {
        get
        {
            return position.height;
        }
    }

    private float ButtonWidth
    {
        get
        {
            return _buttonWidth;
        }
    }

    private float ButtonHeight
    {
        get
        {
            return _buttonHeight;
        }
    }

    private int FontSize
    {
        get
        {
            return 12;
        }
    }

    private float ResizerHeight
    {
        get
        {
            return 4.0f;
        }
    }

    private float MinHeightOfTopAndBottom
    {
        get
        {
            return 60.0f;
        }
    }

    private Color SizerLineCenterColour
    {
        get
        {
            return BluConsoleEditorHelper.ColorFromRGB(162, 162, 162);
        }
    }

    private Color SizerLineBorderColour
    {
        get
        {
            return BluConsoleEditorHelper.ColorFromRGB(130, 130, 130);
        }
    }

    private Color EvenButtonColor
    {
        get
        {
            return BluConsoleEditorHelper.ColorFromRGB(222, 222, 222);
        }
    }

    private Color OddButtonColor
    {
        get
        {
            return BluConsoleEditorHelper.ColorFromRGB(216, 216, 216);
        }
    }

    private Color EvenErrorButtonColor
    {
        get
        {
            return BluConsoleEditorHelper.ColorFromRGB(230, 173, 165);
        }
    }

    private Color OddErrorButtonColor
    {
        get
        {
            return BluConsoleEditorHelper.ColorFromRGB(229, 180, 174);
        }
    }

    private Texture2D SizeLinerCenterTexture
    {
        get
        {
            return BluConsoleEditorHelper.GetTexture(SizerLineCenterColour);
        }
    }

    private Texture2D SizeLinerBorderTexture
    {
        get
        {
            return BluConsoleEditorHelper.GetTexture(SizerLineBorderColour);
        }
    }

    private Texture2D EvenButtonTexture
    {
        get
        {
            return BluConsoleEditorHelper.GetTexture(EvenButtonColor);
        }
    }

    private Texture2D OddButtonTexture
    {
        get
        {
            return BluConsoleEditorHelper.GetTexture(OddButtonColor);
        }
    }

    private Texture2D EvenErrorButtonTexture
    {
        get
        {
            return BluConsoleEditorHelper.GetTexture(EvenErrorButtonColor);
        }
    }

    private Texture2D OddErrorButtonTexture
    {
        get
        {
            return BluConsoleEditorHelper.GetTexture(OddErrorButtonColor);
        }
    }

    private GUIStyle EvenButtonStyle
    {
        get
        {
            return _evenButtonStyle;
        }
    }

    private GUIStyle OddButtonStyle
    {
        get
        {
            return _oddButtonStyle;
        }
    }

    private GUIStyle EvenErrorButtonStyle
    {
        get
        {
            return _evenErrorButtonStyle;
        }
    }

    private GUIStyle OddButtonErrorStyle
    {
        get
        {
            return _oddButtonErrorStyle;
        }
    }

    private GUIStyle SelectedButtonStyle
    {
        get
        {
            return _selectedButtonStyle;
        }
    }

}

}

#endif
