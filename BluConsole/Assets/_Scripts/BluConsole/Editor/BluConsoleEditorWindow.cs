#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


namespace BluConsole.Editor
{

public class BluConsoleEditorWindow : EditorWindow
{
    // LoggerClient
    private LoggerAssetClient _loggerAsset;

    // Cache variables
    private GUIStyle _evenLogLineStyle = null;
    private GUIStyle _oddLogLineStyle = null;
    private GUIStyle _logLineSelectedStyle = null;
    private GUIStyle _evenLogLineErrorStyle = null;
    private GUIStyle _oddLogLineErrorStyle = null;
    private Texture2D _evenLogLineTexture = null;
    private Texture2D _oddLogLineTexture = null;
    private Texture2D _evenLogLineErrorTexture = null;
    private Texture2D _oddLogLineErrorTexture = null;
    private int _logLineWidth = 35;
    private int _logLineHeight = 35;

    // Toolbar Variables
    private bool _isShowNormal = true;
    private bool _isShowWarnings = true;
    private bool _isShowErrors = true;
    private string _searchString = "";

    // LogList Variables
    private Vector2 _logListBeginPosition;
    private int _logListSelectedMessage = -1;
    private double _logListLastTimeClicked = 0.0;

    // Resizer
    private float _topPanelHeight = 100.0f;
    private Rect _cursorChangeRect;
    private bool _isResizing = false;

    // LogDetail Variables
    private Vector2 _logDetailBeginPosition;
    private int _logDetailSelectedFrame = -1;
    private double _logDetailLastTimeClicked = 0.0;

    // Compiling
    private bool _isCompiling = false;
    private List<LogInfo> _dirtyLogsBeforeCompile = new List<LogInfo>();

    [MenuItem("Window/BluConsole")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow<BluConsoleEditorWindow>("BluConsole");
        window._topPanelHeight = window.position.height / 2.0f;
    }

    private void OnGUI()
    {
        Application.logMessageReceived -= LoggerServer.UnityLogHandler;
        Application.logMessageReceived += LoggerServer.UnityLogHandler;

        if (EditorApplication.isCompiling) {
            if (!_isCompiling) {
                _isCompiling = true;
                BeforeCompile();
            }
        } else if (_isCompiling) {
            _isCompiling = false;
            AfterCompile();
        }

        InitCachedVariables();

        GUILayout.BeginVertical(GUILayout.Height(_topPanelHeight), GUILayout.MinHeight(100.0f));
        DrawTopToolbar();
        DrawLogList();
        GUILayout.EndVertical();

        DrawResizer();
        GUILayout.Space(8.0f);

        GUILayout.BeginVertical();
        DrawLogDetail();
        GUILayout.EndVertical();

        Repaint();
    }

    private void BeforeCompile()
    {
        _dirtyLogsBeforeCompile = new List<LogInfo>(_loggerAsset.LogsInfo.Where(log => log.IsCompilerError));
    }

    private void AfterCompile()
    {
        _loggerAsset.Clear(log =>
                           _dirtyLogsBeforeCompile.Where(dirtyLog => dirtyLog.Identifier == log.Identifier).Count() > 0);
    }

    private void InitCachedVariables()
    {
        if (_evenLogLineTexture == null)
            _evenLogLineTexture = BluConsoleEditorHelper.GetTexture(_logLineWidth, _logLineHeight, EvenButtonColor);

        if (_oddLogLineErrorTexture == null)
            _oddLogLineTexture = BluConsoleEditorHelper.GetTexture(_logLineWidth, _logLineHeight, OddButtonColor);

        if (_evenLogLineErrorTexture == null) {
            Color evenLogLineErrorColor = BluConsoleEditorHelper.ColorFromRGB(230, 173, 165);
            _evenLogLineErrorTexture = BluConsoleEditorHelper.GetTexture(_logLineWidth, _logLineHeight, evenLogLineErrorColor);
        }

        if (_oddLogLineErrorTexture == null) {
            Color oddLogLineErrorColor = BluConsoleEditorHelper.ColorFromRGB(229, 180, 174);
            _oddLogLineErrorTexture = BluConsoleEditorHelper.GetTexture(_logLineWidth, _logLineHeight, oddLogLineErrorColor);
        }

        _evenLogLineStyle = new GUIStyle(EditorStyles.label);
        _evenLogLineStyle.richText = true;
        _evenLogLineStyle.fontSize = 13;
        _evenLogLineStyle.alignment = TextAnchor.MiddleLeft;
        _evenLogLineStyle.margin = new RectOffset(0, 0, 0, 0);
        _evenLogLineStyle.padding = new RectOffset(7, 0, 0, 0);
        _evenLogLineStyle.normal.background = _evenLogLineTexture;
        _evenLogLineStyle.active = _evenLogLineStyle.normal;
        _evenLogLineStyle.hover = _evenLogLineStyle.normal;
        _evenLogLineStyle.focused = _evenLogLineStyle.normal;

        _oddLogLineStyle = new GUIStyle(EditorStyles.label);
        _oddLogLineStyle.richText = true;
        _oddLogLineStyle.fontSize = 13;
        _oddLogLineStyle.alignment = TextAnchor.MiddleLeft;
        _oddLogLineStyle.margin = new RectOffset(0, 0, 0, 0);
        _oddLogLineStyle.padding = new RectOffset(7, 0, 0, 0);
        _oddLogLineStyle.normal.background = _oddLogLineTexture;
        _oddLogLineStyle.active = _oddLogLineStyle.normal;
        _oddLogLineStyle.hover = _oddLogLineStyle.normal;
        _oddLogLineStyle.focused = _oddLogLineStyle.normal;

        _logLineSelectedStyle = new GUIStyle(EditorStyles.whiteLabel);
        _logLineSelectedStyle.richText = true;
        _logLineSelectedStyle.fontSize = 13;
        _logLineSelectedStyle.alignment = TextAnchor.MiddleLeft;
        _logLineSelectedStyle.margin = new RectOffset(0, 0, 0, 0);
        _logLineSelectedStyle.padding = new RectOffset(7, 0, 0, 0);
        _logLineSelectedStyle.normal.background = BluConsoleEditorHelper.BlueSelectedBackground;
        _logLineSelectedStyle.active = _logLineSelectedStyle.normal;
        _logLineSelectedStyle.hover = _logLineSelectedStyle.normal;
        _logLineSelectedStyle.focused = _logLineSelectedStyle.normal;

        _evenLogLineErrorStyle = new GUIStyle(EditorStyles.label);
        _evenLogLineErrorStyle.richText = true;
        _evenLogLineErrorStyle.fontSize = 13;
        _evenLogLineErrorStyle.alignment = TextAnchor.MiddleLeft;
        _evenLogLineErrorStyle.margin = new RectOffset(0, 0, 0, 0);
        _evenLogLineErrorStyle.padding = new RectOffset(7, 0, 0, 0);
        _evenLogLineErrorStyle.normal.background = _evenLogLineErrorTexture;
        _evenLogLineErrorStyle.active = _evenLogLineErrorStyle.normal;
        _evenLogLineErrorStyle.hover = _evenLogLineErrorStyle.normal;
        _evenLogLineErrorStyle.focused = _evenLogLineErrorStyle.normal;

        _oddLogLineErrorStyle = new GUIStyle(EditorStyles.label);
        _oddLogLineErrorStyle.richText = true;
        _oddLogLineErrorStyle.fontSize = 13;
        _oddLogLineErrorStyle.alignment = TextAnchor.MiddleLeft;
        _oddLogLineErrorStyle.margin = new RectOffset(0, 0, 0, 0);
        _oddLogLineErrorStyle.padding = new RectOffset(7, 0, 0, 0);
        _oddLogLineErrorStyle.normal.background = _oddLogLineErrorTexture;
        _oddLogLineErrorStyle.active = _oddLogLineErrorStyle.normal;
        _oddLogLineErrorStyle.hover = _oddLogLineErrorStyle.normal;
        _oddLogLineErrorStyle.focused = _oddLogLineErrorStyle.normal;
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

        _loggerAsset.IsClearOnPlay = BluConsoleEditorHelper.ToggleClamped(_loggerAsset.IsClearOnPlay,
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
        _isShowNormal =
            BluConsoleEditorHelper.ToggleClamped(_isShowNormal,
                                                 BluConsoleEditorHelper.InfoGUIContent(_loggerAsset.QtNormalLogs),
                                                 EditorStyles.toolbarButton);
        _isShowWarnings =
            BluConsoleEditorHelper.ToggleClamped(_isShowWarnings,
                                                 BluConsoleEditorHelper.WarningGUIContent(_loggerAsset.QtWarningsLogs),
                                                 EditorStyles.toolbarButton);
        _isShowErrors =
            BluConsoleEditorHelper.ToggleClamped(_isShowErrors,
                                                 BluConsoleEditorHelper.ErrorGUIContent(_loggerAsset.QtErrorsLogs),
                                                 EditorStyles.toolbarButton);


        GUILayout.EndHorizontal();
    }

    private void DrawLogList()
    {
        _logListBeginPosition = GUILayout.BeginScrollView(_logListBeginPosition);

        var logListHeight = WindowHeight;
        var buttonY = 0.0f;
        var buttonHeight = LogListLineHeight;
        var drawnButtons = 0;

        // Filtering by SearchString
        var logsInfo =
            string.IsNullOrEmpty(_searchString) ? _loggerAsset.LogsInfo : _loggerAsset.GetLogsInfoFiltered(_searchString);

        // Filtering by type of log
        logsInfo = logsInfo.Where(log => (((int)log.LogType) & LogTypeMask) != 0).ToList();

        for (int i = 0; i < logsInfo.Count; i++) {
            // Don't draw buttons that are TOTALLY outside of the Window View
            if (buttonY + buttonHeight < _logListBeginPosition.y ||
                buttonY > _logListBeginPosition.y + logListHeight) {
                GUILayout.Space(buttonHeight);
                buttonY += buttonHeight;
                drawnButtons++;
                continue;
            }

            LogInfo logInfo = logsInfo[i];
            string showMessage = null;
            if (logInfo.IsCompilerError) {
                showMessage = logInfo.RawMessage;
            } else {
                showMessage = logInfo.Message.Replace(System.Environment.NewLine, " ");
            }
            showMessage = "  " + showMessage;
            var content = new GUIContent(showMessage, GetIcon(logInfo.LogType));


            var actualLogLineStyle = _evenLogLineStyle;
            if (logInfo.IsCompilerError && i != _logListSelectedMessage) {
                actualLogLineStyle = drawnButtons % 2 == 0 ? _evenLogLineErrorStyle : _oddLogLineErrorStyle;
            } else {
                actualLogLineStyle = i == _logListSelectedMessage ?
                    _logLineSelectedStyle : (drawnButtons % 2 == 0 ? _evenLogLineStyle : _oddLogLineStyle);
            }


            if (GUILayout.Button(content, actualLogLineStyle, GUILayout.Height(buttonHeight))) {
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

            buttonY += buttonHeight;
            drawnButtons++;
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawResizer()
    {
        _cursorChangeRect = new Rect(0, _topPanelHeight, position.width, 5.0f);

        var oldColor = GUI.color;
        GUI.color = SizerLineColour;
        GUI.DrawTexture(_cursorChangeRect, EditorGUIUtility.whiteTexture);
        EditorGUIUtility.AddCursorRect(_cursorChangeRect, MouseCursor.ResizeVertical);
        GUI.color = oldColor;

        if (Event.current.type == EventType.mouseDown && _cursorChangeRect.Contains(Event.current.mousePosition))
            _isResizing = true;
        else if (Event.current.type == EventType.MouseUp)
            _isResizing = false;

        if (_isResizing) {
            _topPanelHeight = Event.current.mousePosition.y;
            _cursorChangeRect.Set(_cursorChangeRect.x, _topPanelHeight, _cursorChangeRect.width, _cursorChangeRect.height);
        }

        _topPanelHeight = Mathf.Clamp(_topPanelHeight, 100, position.height - 100);
    }

    private void DrawLogDetail()
    {
        if (_logListSelectedMessage == -1 ||
            _loggerAsset.LogsInfo.Count == 0 ||
            _loggerAsset.LogsInfo.Count < _logListSelectedMessage + 1)
            return;

        _logDetailBeginPosition = GUILayout.BeginScrollView(_logDetailBeginPosition);

        var log = _loggerAsset.LogsInfo[_logListSelectedMessage];
        for (int i = 0; i < log.CallStack.Count; i++) {
            var frame = log.CallStack[i];
            var methodName = frame.FormattedMethodName;

            if (log.IsCompilerError)
                methodName = log.RawMessage;

            var actualLogLineStyle = i == _logDetailSelectedFrame ?
                _logLineSelectedStyle : (i % 2 == 0 ? _evenLogLineStyle : _oddLogLineStyle);

            if (GUILayout.Button(methodName, actualLogLineStyle)) {
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

    private float LogListLineHeight
    {
        get
        {
            return _evenLogLineStyle.CalcSize(new GUIContent("Test")).y + 15.0f;
        }
    }

    private Color SizerLineColour
    {
        get
        {
            Color defaultBackgroundColor = GUI.backgroundColor;
            return new Color(defaultBackgroundColor.r * 0.5f,
                             defaultBackgroundColor.g * 0.5f,
                             defaultBackgroundColor.b * 0.5f);
        }
    }

    private Color EvenButtonColor
    {
        get
        {
            Color defaultBackgroundColor = GUI.backgroundColor;
            return new Color(defaultBackgroundColor.r * 0.87f,
                             defaultBackgroundColor.g * 0.87f,
                             defaultBackgroundColor.b * 0.87f);
        }
    }

    private Color OddButtonColor
    {
        get
        {
            Color defaultBackgroundColor = GUI.backgroundColor;
            return new Color(defaultBackgroundColor.r * 0.847f,
                             defaultBackgroundColor.g * 0.847f,
                             defaultBackgroundColor.b * 0.847f);
        }
    }

}

}

#endif
