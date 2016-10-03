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
    private Texture2D _evenTexture;
    private Texture2D _oddTexture;
    private Texture2D _evenErrorTexture;
    private Texture2D _oddErrorTexture;
    private GUIStyle _logLineStyle;
    private GUIStyle _logLineSelectedStyle;
    private GUIStyle _logLineErrorStyle;

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

    [MenuItem("Window/BluConsole")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow<BluConsoleEditorWindow>("BluConsole");
        window._topPanelHeight = window.position.height / 2.0f;
    }

    private void OnGUI()
    {
        if (EditorApplication.isCompiling)
            _loggerAsset.ClearCompileErrors();

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

    private void OnEnable()
    {
        if (_loggerAsset == null) {
            _loggerAsset = LoggerServer.GetLoggerClient<LoggerAssetClient>() as LoggerAssetClient;
            if (_loggerAsset == null)
                _loggerAsset = LoggerAssetClient.GetOrCreate();
        }

        ClearAllSelectedMessages();
            
        LoggerServer.Register(_loggerAsset);
    }

    private void DrawTopToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);


        // Clear/Collapse/ClearOnPlay/ErrorPause Area
        if (BluConsoleEditorHelper.ButtonClamped("Clear", EditorStyles.toolbarButton)) {
            _loggerAsset.Clear();
            ClearAllSelectedMessages();
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
        var oldColor = GUI.color;
        GUI.color = ConsoleEvenButtonColor;

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
            var showMessage = logInfo.Message.Replace(System.Environment.NewLine, " ");
            showMessage = "  " + showMessage;
            var content = new GUIContent(showMessage, GetIcon(logInfo.LogType));


            var actualLogLineStyle = LogLineStyle;
            if (logInfo.IsCompilerError && i != _logListSelectedMessage) {
                actualLogLineStyle = GetLogLineNormalButtonErrorStyle(drawnButtons % 2 == 0);
            } else {
                actualLogLineStyle = i == _logListSelectedMessage ? 
                    GetLogLineSelectedButtonStyle() : 
                    GetLogLineNormalButtonStyle(drawnButtons % 2 == 0);
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

        GUI.color = oldColor;
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

        var oldColor = GUI.color;
        GUI.color = ConsoleEvenButtonColor;

        _logDetailBeginPosition = GUILayout.BeginScrollView(_logDetailBeginPosition);
        var log = _loggerAsset.LogsInfo[_logListSelectedMessage];
        for (int i = 0; i < log.CallStack.Count; i++) {
            var frame = log.CallStack[i];
            var methodName = frame.FormattedMethodName;

            if (log.IsCompilerError)
                methodName = log.RawMessage;

            var actualLogLineStyle = i == _logDetailSelectedFrame ? 
                GetLogLineSelectedButtonStyle() : 
                GetLogLineNormalButtonStyle(i % 2 == 0);

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

        GUI.color = oldColor;
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

    private void ClearAllSelectedMessages()
    {
        _logListSelectedMessage = -1;
        _logDetailSelectedFrame = -1;
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
            return LogLineStyle.CalcSize(new GUIContent("Test")).y + 15.0f;
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

    private Color ConsoleEvenButtonColor
    {
        get
        {
            Color defaultBackgroundColor = GUI.backgroundColor;
            return new Color(defaultBackgroundColor.r * 0.87f, 
                             defaultBackgroundColor.g * 0.87f, 
                             defaultBackgroundColor.b * 0.87f);
        }
    }

    private Color ConsoleOddButtonColor
    {
        get
        {
            Color defaultBackgroundColor = GUI.backgroundColor;
            return new Color(defaultBackgroundColor.r * 0.847f, 
                             defaultBackgroundColor.g * 0.847f, 
                             defaultBackgroundColor.b * 0.847f);
        }
    }

    private Texture2D EvenErrorTexture
    {
        get
        {
            if (_evenErrorTexture == null) {
                float width, height;
                LogLineStyle.CalcMinMaxWidth(new GUIContent("Test"), out width, out height);
                Color color = BluConsoleEditorHelper.ColorFromRGB(230, 173, 165);
                _evenErrorTexture = BluConsoleEditorHelper.GetTexture((int)width, (int)height, color);
            }

            return _evenErrorTexture;
        }
    }

    private Texture2D OddErrorTexture
    {
        get
        {
            if (_oddErrorTexture == null) {
                float width, height;
                LogLineStyle.CalcMinMaxWidth(new GUIContent("Test"), out width, out height);
                Color color = BluConsoleEditorHelper.ColorFromRGB(229, 180, 174);
                _oddErrorTexture = BluConsoleEditorHelper.GetTexture((int)width, (int)height, color);
            }

            return _oddErrorTexture;
        }
    }

    private GUIStyle LogLineStyle
    {
        get
        {
            if (_logLineStyle == null) {
                _logLineStyle = new GUIStyle(EditorStyles.label);
                _logLineStyle.richText = true;
                _logLineStyle.fontSize = 13;
                _logLineStyle.alignment = TextAnchor.MiddleLeft;
                _logLineStyle.margin = new RectOffset(0, 0, 0, 0);
                _logLineStyle.padding = new RectOffset(7, 0, 0, 0);
                _logLineStyle.active = _logLineStyle.normal;
                _logLineStyle.hover = _logLineStyle.normal;
                _logLineStyle.focused = _logLineStyle.normal;
            }
            return _logLineStyle;
        }
    }

    private GUIStyle LogLineSelectedStyle
    {
        get
        {
            if (_logLineSelectedStyle == null) {
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
            }
            return _logLineSelectedStyle;
        }
    }

    private GUIStyle LogLineErrorStyle
    {
        get
        {
            if (_logLineErrorStyle == null) {
                _logLineErrorStyle = new GUIStyle(EditorStyles.label);
                _logLineErrorStyle.richText = true;
                _logLineErrorStyle.fontSize = 13;
                _logLineErrorStyle.alignment = TextAnchor.MiddleLeft;
                _logLineErrorStyle.margin = new RectOffset(0, 0, 0, 0);
                _logLineErrorStyle.padding = new RectOffset(7, 0, 0, 0);
                _logLineErrorStyle.active = _logLineErrorStyle.normal;
                _logLineErrorStyle.hover = _logLineErrorStyle.normal;
                _logLineErrorStyle.focused = _logLineErrorStyle.normal;
            }
            return _logLineErrorStyle;
        }
    }

    private GUIStyle GetLogLineSelectedButtonStyle()
    {
        return LogLineSelectedStyle;
    }

    private GUIStyle GetLogLineNormalButtonStyle(bool isEven)
    {
        GUIStyle style = LogLineStyle;

        float width, height;
        style.CalcMinMaxWidth(new GUIContent("Test"), out width, out height);

        if (isEven) {
            if (_evenTexture == null)
                _evenTexture = BluConsoleEditorHelper.GetTexture((int)width, (int)height, ConsoleEvenButtonColor);
            style.normal.background = _evenTexture;
        } else {
            if (_oddTexture == null)
                _oddTexture = BluConsoleEditorHelper.GetTexture((int)width, (int)height, ConsoleOddButtonColor);
            style.normal.background = _oddTexture;
        }

        return style;
    }

    private GUIStyle GetLogLineNormalButtonErrorStyle(bool isEven)
    {
        GUIStyle style = LogLineErrorStyle;

        float width, height;
        style.CalcMinMaxWidth(new GUIContent("Test"), out width, out height);
        style.normal.background = isEven ? EvenErrorTexture : OddErrorTexture;
        return style;
    }

}

}

#endif