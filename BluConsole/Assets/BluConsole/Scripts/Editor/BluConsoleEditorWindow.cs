/*
  MIT License

  Copyright (c) [2016] [Gilberto Alexandre dos Santos]

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all
  copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
  SOFTWARE.
*/


using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluConsole;
using BluConsole.Core;
using BluConsole.Core.UnityLoggerApi;


namespace BluConsole.Editor
{

public class BluConsoleEditorWindow : EditorWindow
{
	private readonly int MAX_LOGS = 999;
	private readonly int MAX_LENGTH_MESSAGE = 999;
	private readonly int MAX_LENGTH_COLLAPSE = 999;

	// Layout variables
	private float _drawYPos;

	// Cache Variables
	private Texture2D _consoleIcon;
	private bool _hasConsoleIcon = true;
	private float _buttonWidth;
	private float _buttonHeight;
    private UnityApiEvents _unityApiEvents;
    private List<BluLog> _cacheLog = new List<BluLog>();
    private List<bool> _cacheLogComparer = new List<bool>();
    private List<string> _stackTraceIgnorePrefixs = new List<string>();
    private BluLogSettings _settings;
    private int _cacheLogCount = 0;

	// Toolbar Variables
    private string[] _searchStringPatterns;
	private string _searchString = "";

	// LogList Variables
	private Vector2 _logListScrollPosition;
	private int _logListSelectedMessage = -1;
	private double _logListLastTimeClicked = 0.0;
    private int _qtLogs = 0;

    // Repaint logic
    private bool _needRepaint;

	// Resizer
	private float _topPanelHeight;
	private Rect _cursorChangeRect;
	private bool _isResizing = false;

	// LogDetail Variables
	private Vector2 _logDetailScrollPosition;
	private int _logDetailSelectedFrame = -1;
	private double _logDetailLastTimeClicked = 0.0;
    private BluLog _selectedLog = null;

	// Scroll Logic
	private bool _isFollowScroll = false;
	private bool _hasScrollWheelUp = false;

	[MenuItem("Window/BluConsole")]
	public static void ShowWindow()
	{
		var window = EditorWindow.GetWindow<BluConsoleEditorWindow>("BluConsole");

		var consoleIcon = window.ConsoleIcon;
		if (consoleIcon != null)
			window.titleContent = new GUIContent("BluConsole", consoleIcon);
		else
			window.titleContent = new GUIContent("BluConsole");

		window._topPanelHeight = window.position.height / 2.0f;
	}

    private void OnEnable()
    {
        _stackTraceIgnorePrefixs = GetStackTraceIgnorePrefixs();
        _stackTraceIgnorePrefixs.AddRange(GetDefaultIgnorePrefixs());

        _settings = GetOrCreateSettings();
        _settings.CacheFilterLower();

        if (_unityApiEvents == null)
            _unityApiEvents = UnityApiEvents.GetOrCreate();
        
        SetDirtyLogs();
    }

    private BluLogSettings GetOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<BluLogSettings>("Assets/BluConsole/Assets/BluLogSettings.asset");
        return settings ?? CreateInstance<BluLogSettings>();
    }

    private void Update()
    {
        int qt = UnityLoggerServer.GetCount();
        if (qt != _qtLogs)
            _needRepaint = true;

        _unityApiEvents.OnBeforeCompileEvent -= SetDirtyLogs;
        _unityApiEvents.OnBeforeCompileEvent += SetDirtyLogs;
        _unityApiEvents.OnAfterCompileEvent -= SetDirtyLogs;
        _unityApiEvents.OnAfterCompileEvent += SetDirtyLogs;
        _unityApiEvents.OnBeginPlayEvent -= SetDirtyLogs;
        _unityApiEvents.OnBeginPlayEvent += SetDirtyLogs;
        _unityApiEvents.OnStopPlayEvent -= SetDirtyLogs;
        _unityApiEvents.OnStopPlayEvent += SetDirtyLogs;

        if (_needRepaint)
        {
            Repaint();
            _needRepaint = false;
        }
    }

	private void OnGUI()
	{
		InitVariables();

		DrawResizer();

		_hasScrollWheelUp = Event.current.type == EventType.ScrollWheel && Event.current.delta.y < 0f;

		GUILayout.BeginVertical(GUILayout.Height(_topPanelHeight), GUILayout.MinHeight(MinHeightOfTopAndBottom));

		_drawYPos += DrawTopToolbar();
		_drawYPos -= 1f;
		DrawLogList();

		GUILayout.EndVertical();

		GUILayout.Space(ResizerHeight);

		GUILayout.BeginVertical(GUILayout.Height(WindowHeight - _topPanelHeight - ResizerHeight));
		_drawYPos = _topPanelHeight + ResizerHeight;
		DrawLogDetail();

		GUILayout.EndVertical();
	}

	private void InitVariables()
	{
		_buttonWidth = position.width;
		_buttonHeight = BluConsoleSkin.MessageStyle.CalcSize(new GUIContent("Test")).y + 15.0f;
		_drawYPos = 0f;
	}

	private float DrawTopToolbar()
	{
		float height = BluConsoleSkin.ToolbarButtonStyle.CalcSize(new GUIContent("Clear")).y;
		GUILayout.BeginHorizontal(BluConsoleSkin.ToolbarStyle, GUILayout.Height(height));

		if (GetButtonClamped("Clear", BluConsoleSkin.ToolbarButtonStyle))
		{
            UnityLoggerServer.Clear();
            SetDirtyLogs();
			_logListSelectedMessage = -1;
			_logDetailSelectedFrame = -1;
		}

		GUILayout.Space(6.0f);

        bool oldCollapse = UnityLoggerServer.HasFlag(ConsoleWindowFlag.Collapse);
        bool newCollapse = GetToggleClamped(oldCollapse, "Collapse", BluConsoleSkin.ToolbarButtonStyle);
        if (oldCollapse != newCollapse)
            SetDirtyLogs();
        UnityLoggerServer.SetFlag(ConsoleWindowFlag.Collapse, newCollapse);


        bool oldClearOnPlay = UnityLoggerServer.HasFlag(ConsoleWindowFlag.ClearOnPlay);
        bool newClearOnPlay = GetToggleClamped(oldClearOnPlay, "Clear on Play", BluConsoleSkin.ToolbarButtonStyle);
        UnityLoggerServer.SetFlag(ConsoleWindowFlag.ClearOnPlay, newClearOnPlay);


        bool oldPauseOnError = UnityLoggerServer.HasFlag(ConsoleWindowFlag.ErrorPause);
        bool newPauseOnError = GetToggleClamped(oldPauseOnError, "Pause on Error", BluConsoleSkin.ToolbarButtonStyle);
        UnityLoggerServer.SetFlag(ConsoleWindowFlag.ErrorPause, newPauseOnError);

		GUILayout.FlexibleSpace();

		// Search Area
        var oldString = _searchString;
		_searchString = EditorGUILayout.TextArea(_searchString,
		                                         BluConsoleSkin.ToolbarSearchTextFieldStyle,
		                                         GUILayout.Width(200.0f));
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

		int maxLogs = MAX_LOGS;
		string qtNormalLogsStr = qtNormalLogs.ToString();
		if (qtNormalLogs >= maxLogs)
			qtNormalLogsStr = maxLogs.ToString() + "+";

		string qtWarningLogsStr = qtWarningLogs.ToString();
		if (qtWarningLogs >= maxLogs)
			qtWarningLogsStr = maxLogs.ToString() + "+";

		string qtErrorLogsStr = qtErrorLogs.ToString();
		if (qtErrorLogs >= maxLogs)
			qtErrorLogsStr = maxLogs.ToString() + "+";


        bool oldIsShowNormal = UnityLoggerServer.HasFlag(ConsoleWindowFlag.LogLevelLog);
        bool newIsShowNormal = GetToggleClamped(oldIsShowNormal, 
                                                GetInfoGUIContent(qtNormalLogsStr), 
                                                BluConsoleSkin.ToolbarButtonStyle);
        if (oldIsShowNormal != newIsShowNormal)
            SetDirtyLogs();
        UnityLoggerServer.SetFlag(ConsoleWindowFlag.LogLevelLog, newIsShowNormal);


        bool oldIsShowWarning = UnityLoggerServer.HasFlag(ConsoleWindowFlag.LogLevelWarning);
        bool newIsShowWarning = GetToggleClamped(oldIsShowWarning, 
                                                 GetWarningGUIContent(qtWarningLogsStr), 
                                                 BluConsoleSkin.ToolbarButtonStyle);
        if (oldIsShowWarning != newIsShowWarning)
            SetDirtyLogs();
        UnityLoggerServer.SetFlag(ConsoleWindowFlag.LogLevelWarning, newIsShowWarning);


        bool oldIsShowError = UnityLoggerServer.HasFlag(ConsoleWindowFlag.LogLevelError);
        bool newIsShowError = GetToggleClamped(oldIsShowError, 
                                               GetErrorGUIContent(qtErrorLogsStr), 
                                               BluConsoleSkin.ToolbarButtonStyle);
        if (oldIsShowError != newIsShowError)
            SetDirtyLogs();
        UnityLoggerServer.SetFlag(ConsoleWindowFlag.LogLevelError, newIsShowError);

		GUILayout.EndHorizontal();

		return height;
	}

    private void SetDirtyLogs()
    {
        _cacheLog.Clear();
        _cacheLogCount = 0;
        _needRepaint = true;
        SetDirtyComparer();
    }

    private void SetDirtyComparer()
    {
        _cacheLogComparer.Clear();
    }

	private void DrawLogList()
	{
        _qtLogs = UnityLoggerServer.StartGettingLogs();
        _cacheLogCount = _qtLogs;

        int cntLogs = 0;
        List<int> rows = new List<int>(_qtLogs);
        List<BluLog> logs = new List<BluLog>(_qtLogs);
        for (int i = 0; i < _qtLogs; i++)
        {
            var log = GetSimpleLog(i);
            if (HasPattern(log, i))
            {
                cntLogs++;
                rows.Add(i);
                logs.Add(log);
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

		float buttonY = firstRenderLogIndex * ButtonHeight;
		bool hasSomeClick = false;

        int cnt = 0;
        for (int i = firstRenderLogIndex; i < lastRenderLogIndex; i++)
        {
            var row = rows[i];
            var log = logs[i];
            var styleBack = GetLogBackStyle(cnt, log);

            var styleMessage = GetLogListStyle(log);
            string showMessage = GetTruncatedMessage(GetLogListMessage(log));
			var contentMessage = new GUIContent(showMessage);
			var rectMessage = new Rect(x: 0,
			                           y: buttonY,
			                           width: viewWidth,
			                           height: ButtonHeight);
            bool isSelected = i == _logListSelectedMessage ? true : false;
			DrawBack(rectMessage, styleBack, isSelected);
			if (IsRepaintEvent)
				styleMessage.Draw(rectMessage, contentMessage, false, false, isSelected, false);
			
			bool messageClicked = IsClicked(rectMessage);
			bool isLeftClick = messageClicked ? Event.current.button == 0 : false;

            if (UnityLoggerServer.HasFlag(ConsoleWindowFlag.Collapse))
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
                _needRepaint = true;
                _selectedLog = GetCompleteLog(row);

				hasSomeClick = true;

                if (!isLeftClick && i == _logListSelectedMessage)
					DrawPopup(Event.current, log);

                if (isLeftClick && i == _logListSelectedMessage)
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
                    _logListSelectedMessage = i;
				}

				_logDetailSelectedFrame = -1;
			}

			buttonY += ButtonHeight;
            cnt++;
		}

        UnityLoggerServer.StopGettingsLogs();

		GUI.EndScrollView();


		if (_hasScrollWheelUp || hasSomeClick)
		{
			_isFollowScroll = false;
		}
		else if (_logListScrollPosition != oldScrollPosition)
		{
			_isFollowScroll = false;
			float topOffset = viewHeight - windowHeight;
			if (_logListScrollPosition.y >= topOffset)
				_isFollowScroll = true;
		}

		if (!IsFollowScroll)
			return;

		float endY = viewHeight - windowHeight;
		_logListScrollPosition.y = endY;
	}

	private void DrawResizer()
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
            _needRepaint = true;
			_topPanelHeight = Event.current.mousePosition.y;
			_cursorChangeRect.Set(_cursorChangeRect.x, resizerY, _cursorChangeRect.width, _cursorChangeRect.height);
		}

		_topPanelHeight = Mathf.Clamp(_topPanelHeight,
		                              MinHeightOfTopAndBottom,
		                              position.height - MinHeightOfTopAndBottom);
	}
		
	private void DrawLogDetail()
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
			return;
        }

        var log = _selectedLog;

        var size = log.StackTrace.Count;
		var sizePlus = size + 1;

		float buttonHeight = GetDetailMessageHeight("A", MessageDetailCallstackStyle);
		float buttonWidth = ButtonWidth;
		float firstLogHeight = Mathf.Max(buttonHeight, GetDetailMessageHeight(GetTruncatedMessage(log.Message), 
		                                                                      MessageDetailFirstLogStyle, 
		                                                                      buttonWidth));

		float viewHeight = size * buttonHeight + firstLogHeight;

		if (viewHeight > windowHeight)
			buttonWidth -= 15f;

		// Recalculate it because we decreased the buttonWdith
		firstLogHeight = Mathf.Max(buttonHeight, GetDetailMessageHeight(GetTruncatedMessage(log.Message), 
		                                                                MessageDetailFirstLogStyle, 
		                                                                buttonWidth));
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
			lastRenderLogIndex = firstRenderLogIndex + (int)((windowHeight / buttonHeight)) + 2;
		}
		lastRenderLogIndex = Mathf.Clamp(lastRenderLogIndex, 0, sizePlus);

		float buttonY = 0f;
		if (firstRenderLogIndex > 0)
			buttonY = firstLogHeight + (firstRenderLogIndex - 1) * buttonHeight;
			
		// Logging first message
		if (firstRenderLogIndex == 0)
		{
			var styleBack = GetLogBackStyle(0, log);
			var styleMessage = MessageDetailFirstLogStyle;
			var rectButton = new Rect(x: 0,  y: buttonY, width: viewWidth, height: firstLogHeight);

			var isSelected = _logDetailSelectedFrame == -2 ? true : false;
			var contentMessage = new GUIContent(GetTruncatedMessage(log.Message));

			DrawBack(rectButton, styleBack, isSelected);
			if (IsRepaintEvent)
				styleMessage.Draw(rectButton, contentMessage, false, false, isSelected, false);

			bool messageClicked = IsClicked(rectButton);
			if (messageClicked)
			{
                _needRepaint = true;
				bool isLeftClick = Event.current.button == 0;

				if (!isLeftClick && _logDetailSelectedFrame == -2)
					DrawPopup(Event.current, log);

				if (isLeftClick && _logDetailSelectedFrame == -2)
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
					_logDetailSelectedFrame = -2;
				}
			}

			buttonY += firstLogHeight;
		}

		for (int i = firstRenderLogIndex == 0 ? 0 : firstRenderLogIndex - 1; i + 1 < lastRenderLogIndex; i++)
		{
            var contentMessage = new GUIContent(GetTruncatedMessage(log.StackTrace[i].FrameInformation));

			var styleBack = GetLogBackStyle(0, log);
			var styleMessage = MessageDetailCallstackStyle;
			var rectButton = new Rect(x: 0, y: buttonY, width: viewWidth, height: buttonHeight);

			var isSelected = i == _logDetailSelectedFrame ? true : false;
			DrawBack(rectButton, styleBack, isSelected);
			if (IsRepaintEvent)
				styleMessage.Draw(rectButton, contentMessage, false, false, isSelected, false);

			bool messageClicked = IsClicked(rectButton);
			if (messageClicked)
			{
                _needRepaint = true;
				bool isLeftClick = Event.current.button == 0;

				if (isLeftClick && i == _logDetailSelectedFrame)
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
					_logDetailSelectedFrame = i;
				}
			}

			buttonY += buttonHeight;
		}

		GUI.EndScrollView();
	}

	private void DrawPopup(
		Event clickEvent,
        BluLog log)
	{
		GenericMenu.MenuFunction copyCallback = () => {
			EditorGUIUtility.systemCopyBuffer = log.Message;
		};

		GenericMenu menu = new GenericMenu();
		menu.AddItem(content: new GUIContent("Copy"), on: false, func: copyCallback);
		menu.ShowAsContext();

		clickEvent.Use();
	}

	private void DrawBack(
		Rect rect,
		GUIStyle style,
		bool isSelected)
	{
		if (IsRepaintEvent)
			style.Draw(rect, false, false, isSelected, false);
	}

	private bool IsClicked(Rect rect)
	{
		return Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition);
	}

    private BluLog GetSimpleLog(int row)
    {
        int realCount = _cacheLog.Count;

        if (row < _cacheLogCount && row < realCount)
            return _cacheLog[row];

        _needRepaint = true;

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

    private void PingLog(BluLog log)
    {
        if (log.InstanceID != 0)
            EditorGUIUtility.PingObject(log.InstanceID);
    }

	#region Gets

	private float GetMaxDetailMessageHeight(
        float normalHeight)
	{
		return normalHeight * 5f;
	}

	private string GetTruncatedMessage(
		string message)
	{
		if (message.Length <= MAX_LENGTH_MESSAGE)
			return message;

		return string.Format("{0}... <truncated>", message.Substring(startIndex: 0, length: MAX_LENGTH_MESSAGE));
	}

	private string GetLogListMessage(
        BluLog log)
	{
		return log.Message.Replace(System.Environment.NewLine, " ");
	}

	private GUIStyle GetLogBackStyle(
        int row,
        BluLog log)
	{
        return row % 2 == 0 ? BluConsoleSkin.EvenBackStyle : BluConsoleSkin.OddBackStyle;
	}

	private GUIStyle GetLogListStyle(
        BluLog log)
	{
        BluLogType logType = GetLogType(log);
        switch (logType)
        {
        case BluLogType.Normal:
            return BluConsoleSkin.LogInfoStyle;
        case BluLogType.Warning:
            return BluConsoleSkin.LogWarnStyle;
        case BluLogType.Error:
            return BluConsoleSkin.LogErrorStyle;
        }
        return BluConsoleSkin.LogInfoStyle;
	}

	private float GetListMessageWidth(
		string message,
		GUIStyle style)
	{
		return style.CalcSize(new GUIContent(message)).x;
	}

	private float GetDetailMessageWidth(
		string message,
		GUIStyle style)
	{
		return style.CalcSize(new GUIContent(message)).x;
	}

	private float GetDetailMessageHeight(
		string message,
		GUIStyle style,
		float width = 0f)
	{
		return style.CalcHeight(new GUIContent(message), width);
	}

	private Texture2D GetIcon(
		BluLogType logType)
	{
		switch (logType)
		{
		case BluLogType.Normal:
			return BluConsoleSkin.InfoIcon;
		case BluLogType.Warning:
			return BluConsoleSkin.WarningIcon;
		case BluLogType.Error:
			return BluConsoleSkin.ErrorIcon;
		default:
			return BluConsoleSkin.InfoIcon;
		}
	}

	private Texture2D GetIconSmall(
		BluLogType logType)
	{
		switch (logType)
		{
		case BluLogType.Normal:
			return BluConsoleSkin.InfoIconSmall;
		case BluLogType.Warning:
			return BluConsoleSkin.WarningIconSmall;
		case BluLogType.Error:
			return BluConsoleSkin.ErrorIconSmall;
		default:
			return BluConsoleSkin.InfoIconSmall;
		}
	}

    private BluLogType GetLogType(
        BluLog log)
    {
        int mode = log.Mode;
        if (UnityLoggerServer.HasMode(mode, (ConsoleWindowMode)GetLogMask(BluLogType.Error)))
            return BluLogType.Error;
        else if (UnityLoggerServer.HasMode(mode, (ConsoleWindowMode)GetLogMask(BluLogType.Warning)))
            return BluLogType.Warning;
        else
            return BluLogType.Normal;
    }

    private int GetLogMask(
        BluLogType type)
    {
        switch (type)
        {
        case BluLogType.Normal:
            return 1028;
        case BluLogType.Warning:
            return 4736;
        default:
            return 3148115;
        }
    }

	private void JumpToSource(
        BluLog log,
        int row)
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
		
        var filename = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), file);
		if (System.IO.File.Exists(filename))
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(file, line);
	}

	private GUIContent GetInfoGUIContent(
		string text)
	{
		return new GUIContent(text, BluConsoleSkin.InfoIconSmall);
	}

	private GUIContent GetInfoGUIContent(
		int value)
	{
		return new GUIContent(value.ToString(), BluConsoleSkin.InfoIconSmall);
	}

	private GUIContent GetWarningGUIContent(
		string text)
	{
		return new GUIContent(text, BluConsoleSkin.WarningIconSmall);
	}

	private GUIContent GetWarningGUIContent(
		int value)
	{
		return new GUIContent(value.ToString(), BluConsoleSkin.WarningIconSmall);
	}

	private GUIContent GetErrorGUIContent(
		string text)
	{
		return new GUIContent(text, BluConsoleSkin.ErrorIconSmall);
	}

	private GUIContent GetErrorGUIContent(
		int value)
	{
		return new GUIContent(value.ToString(), BluConsoleSkin.ErrorIconSmall);
	}

	private bool GetButtonClamped(
		string text,
		GUIStyle style)
	{
		return GUILayout.Button(text, style, GUILayout.MaxWidth(style.CalcSize(new GUIContent(text)).x));
	}

	private bool GetToggleClamped(
		bool state,
		string text,
		GUIStyle style)
	{
		return GUILayout.Toggle(state, text, style, GUILayout.MaxWidth(style.CalcSize(new GUIContent(text)).x));
	}

	private bool GetToggleClamped(
		bool state,
		GUIContent content,
		GUIStyle style)
	{
		return GUILayout.Toggle(state, content, style, GUILayout.MaxWidth(style.CalcSize(content).x));
	}

    private bool HasPattern(
        BluLog log,
        int row)
    {
        if (row < _cacheLogComparer.Count)
            return _cacheLogComparer[row];

        string messageLower = log.MessageLower;

        int size = _searchStringPatterns.Length;
        for (int i = 0; i < size; i++)
        {
            string pattern = _searchStringPatterns[i];
            if (pattern == "")
                continue;

            if (!messageLower.Contains(pattern))
            {
                SetLogComparer(row, false);
                return false;
            }
        }

        if (UnityLoggerServer.IsDebugError(log.Mode))
        {
            SetLogComparer(row, true);
            return true;
        }

        var filters = _settings.FilterLower;
        size = _settings.FilterLower.Count;
        for (int i = 0; i < size; i++)
        {
            var filter = filters[i];
            
            if (!messageLower.Contains(filter))
            {
                SetLogComparer(row, false);
                return false;
            }
        }

        SetLogComparer(row, true);
        return true;
    }

    private void SetLogComparer(int row, bool value)
    {
        if (row < _cacheLogComparer.Count)
            _cacheLogComparer[row] = value;
        _cacheLogComparer.Add(value);
    }

    private List<BluLogFrame> FilterLogFrames(List<BluLogFrame> frames)
    {
        var filteredFrames = new List<BluLogFrame>(frames.Count);
        foreach (var frame in frames)
        {
            bool starts = false;
            foreach (var stackTrace in _stackTraceIgnorePrefixs)
            {
                if (frame.FrameInformation.StartsWith(stackTrace))
                {
                    starts = true;
                    break;
                }
            }
            if (!starts)
                filteredFrames.Add(frame);
        }
        return filteredFrames;
    }

    private List<string> GetDefaultIgnorePrefixs()
    {
        return new List<string>() {
            "UnityEngine.Debug"
        };
    }

    private List<string> GetStackTraceIgnorePrefixs()
    {
        var ret = new List<string>();
        var assembly = Assembly.GetAssembly(typeof(StackTraceIgnore));
        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | 
                                                   BindingFlags.NonPublic |
                                                   BindingFlags.Static |
                                                   BindingFlags.Instance))
            {
                if (method.GetCustomAttributes(typeof(StackTraceIgnore), true).Length > 0)
                {
                    var key = string.Format("{0}:{1}", method.DeclaringType.FullName, method.Name);
                    ret.Add(key);
                }
            }
        }
        return ret;
    }


	#endregion Gets


	#region Properties

	private Texture2D ConsoleIcon
	{
		get
		{
			if (_consoleIcon == null && _hasConsoleIcon)
			{
				string path = "Assets/BluConsole/Images/bluconsole-icon.png";
				_consoleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				if (_consoleIcon == null)
					_hasConsoleIcon = false;
			}

			return _consoleIcon;
		}
	}

	private GUIStyle MessageDetailFirstLogStyle
	{
		get
		{
			var style = new GUIStyle(BluConsoleSkin.MessageStyle);
			style.stretchWidth = true;
			style.wordWrap = true;
			return style;
		}
	}

	private GUIStyle MessageDetailCallstackStyle
	{
		get
		{
			var style = new GUIStyle(MessageDetailFirstLogStyle);
			style.wordWrap = false;
			return style;
		}
	}

	private bool IsRepaintEvent
	{
		get
		{
			return Event.current.type == EventType.Repaint;
		}
	}

	private bool IsFollowScroll
	{
		get
		{
			return _isFollowScroll;
		}
	}

	private bool IsDoubleClickLogListButton
	{
		get
		{
			return (EditorApplication.timeSinceStartup - _logListLastTimeClicked) < 0.3f;
		}
	}

	private bool IsDoubleClickLogDetailButton
	{
		get
		{
			return (EditorApplication.timeSinceStartup - _logDetailLastTimeClicked) < 0.3f;
		}
	}

	private float DrawYPos
	{
		get
		{
			return _drawYPos;
		}
	}

	private float WindowWidth
	{
		get
		{
			return position.width;
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
			return 1.0f;
		}
	}

	private float MinHeightOfTopAndBottom
	{
		get
		{
			return 60.0f;
		}
	}


	#endregion Properties

}

}
