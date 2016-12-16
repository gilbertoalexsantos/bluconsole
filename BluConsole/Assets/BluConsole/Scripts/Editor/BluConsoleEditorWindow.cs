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
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluConsole.Core;


namespace BluConsole.Editor
{

public class BluConsoleEditorWindow : EditorWindow
{
	private readonly int MAX_LOGS = 999;
	private readonly int MAX_LENGTH_MESSAGE = 999;
	private readonly int MAX_LENGTH_COLLAPSE = 999;

	// LoggerClient
	private LoggerAssetClient _loggerAsset;

	// Layout variables
	private float _drawYPos;

	// Cache Variables
	private Texture2D _consoleIcon;
	private bool _hasConsoleIcon = true;

	private List<CountedLog> _countedLogs = new List<CountedLog>();
	private bool _isCountedLogsDirty = true;

	private float _buttonWidth;
	private float _buttonHeight;

	// Toolbar Variables
	private bool _isShowNormal = true;
	private bool _isShowWarnings = true;
	private bool _isShowErrors = true;
	private bool _isClearOnPlay = false;
	private bool _isCollapse = false;
	private string _searchString = "";

	// LogList Variables
	private Vector2 _logListScrollPosition;
	private int _logListSelectedMessage = -1;
	private double _logListLastTimeClicked = 0.0;

	// Resizer
	private float _topPanelHeight;
	private Rect _cursorChangeRect;
	private bool _isResizing = false;

	// LogDetail Variables
	private Vector2 _logDetailScrollPosition;
	private int _logDetailSelectedFrame = -1;
	private double _logDetailLastTimeClicked = 0.0;

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

	private void OnBecameVisible()
	{
		SetCountedLogsDirty();
	}

	private void Update()
	{
		_loggerAsset.OnNewLogOrTrimLogEvent -= OnNewLogOrTrimLog;
		_loggerAsset.OnNewLogOrTrimLogEvent += OnNewLogOrTrimLog;
		_loggerAsset.OnBeforeCompileEvent -= OnBeforeCompile;
		_loggerAsset.OnBeforeCompileEvent += OnBeforeCompile;
		_loggerAsset.OnAfterCompileEvent -= OnAfterCompile;
		_loggerAsset.OnAfterCompileEvent += OnAfterCompile;
		_loggerAsset.OnBeginPlayEvent -= OnBeginPlay;
		_loggerAsset.OnBeginPlayEvent += OnBeginPlay;
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

		Repaint();
	}

	private void InitVariables()
	{
		_buttonWidth = position.width;
		_buttonHeight = BluConsoleSkin.MessageStyle.CalcSize(new GUIContent("Test")).y + 15.0f;
		_drawYPos = 0f;
	}

	public void OnBeforeCompile()
	{
		SetCountedLogsDirty();
	}

	public void OnAfterCompile()
	{
		SetCountedLogsDirty();
	}

	public void OnBeginPlay()
	{
		if (_isClearOnPlay)
		{
			SetCountedLogsDirty();
			_loggerAsset.Clear();
		}
	}

	private void OnEnable()
	{
		if (_loggerAsset == null)
		{
			_loggerAsset = LoggerServer.GetLoggerClient<LoggerAssetClient>() as LoggerAssetClient;
			if (_loggerAsset == null)
				_loggerAsset = LoggerAssetClient.GetOrCreate();
		}

		_logListSelectedMessage = -1;
		_logDetailSelectedFrame = -1;

		LoggerServer.Register(_loggerAsset);
		SetCountedLogsDirty();
	}

	private void OnNewLogOrTrimLog()
	{
		SetCountedLogsDirty();
	}

	private float DrawTopToolbar()
	{
		float height = BluConsoleSkin.ToolbarButtonStyle.CalcSize(new GUIContent("Clear")).y;
		GUILayout.BeginHorizontal(BluConsoleSkin.ToolbarStyle, GUILayout.Height(height));

		// Clear/Collapse/ClearOnPlay/ErrorPause Area
		if (GetButtonClamped("Clear", BluConsoleSkin.ToolbarButtonStyle))
		{
			SetCountedLogsDirty();
			_loggerAsset.ClearExceptCompileErrors();
			_logListSelectedMessage = -1;
			_logDetailSelectedFrame = -1;
		}

		GUILayout.Space(6.0f);

		bool oldCollapseValue = _isCollapse;
		_isCollapse = GetToggleClamped(_isCollapse,
		                               "Collapse",
		                               BluConsoleSkin.ToolbarButtonStyle);
		if (oldCollapseValue != _isCollapse)
			SetCountedLogsDirty();

		_isClearOnPlay = GetToggleClamped(_isClearOnPlay,
		                                  "Clear on Play",
		                                  BluConsoleSkin.ToolbarButtonStyle);

		_loggerAsset.IsPauseOnError = GetToggleClamped(_loggerAsset.IsPauseOnError,
		                                               "Pause on Error",
		                                               BluConsoleSkin.ToolbarButtonStyle);


		GUILayout.FlexibleSpace();

		// Search Area
		string oldSearchString = _searchString;
		_searchString = EditorGUILayout.TextArea(_searchString,
		                                         BluConsoleSkin.ToolbarSearchTextFieldStyle,
		                                         GUILayout.Width(200.0f));
		if (oldSearchString != _searchString)
			SetCountedLogsDirty();

		if (GUILayout.Button("", BluConsoleSkin.ToolbarSearchCancelButtonStyle))
		{
			_searchString = "";
			GUI.FocusControl(null);
			SetCountedLogsDirty();
		}


		GUILayout.Space(10.0f);


		// Info/Warning/Error buttons Area
		int qtNormalLogs = _isCollapse ? _loggerAsset.QtNormalCountedLogs : _loggerAsset.QtNormalLogs;
		int qtWarningLogs = _isCollapse ? _loggerAsset.QtWarningCountedLogs : _loggerAsset.QtWarningLogs;
		int qtErrorLogs = _isCollapse ? _loggerAsset.QtErrorCountedLogs : _loggerAsset.QtErrorLogs;

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

		bool oldIsShowNormal = _isShowNormal;
		_isShowNormal = GetToggleClamped(_isShowNormal, 
		                                 GetInfoGUIContent(qtNormalLogsStr), 
		                                 BluConsoleSkin.ToolbarButtonStyle);
		if (oldIsShowNormal != _isShowNormal)
			SetCountedLogsDirty();


		bool oldIsShowWarnings = _isShowWarnings;
		_isShowWarnings = GetToggleClamped(_isShowWarnings, 
		                                   GetWarningGUIContent(qtWarningLogsStr), 
		                                   BluConsoleSkin.ToolbarButtonStyle);
		if (oldIsShowWarnings != _isShowWarnings)
			SetCountedLogsDirty();


		bool oldIsShowErrors = _isShowErrors;
		_isShowErrors = GetToggleClamped(_isShowErrors, 
		                                 GetErrorGUIContent(qtErrorLogsStr), 
		                                 BluConsoleSkin.ToolbarButtonStyle);
		if (oldIsShowErrors != _isShowErrors)
			SetCountedLogsDirty();


		GUILayout.EndHorizontal();

		return height;
	}

	private void DrawLogList()
	{
		List<CountedLog> logs = CountedLogsFilteredByFlags;

		float windowWidth = WindowWidth;
		float windowHeight = _topPanelHeight - DrawYPos;

		float buttonWidth = ButtonWidth;
		if (logs.Count * ButtonHeight > windowHeight)
			buttonWidth -= 15f;

		float viewWidth = buttonWidth;
		float viewHeight = logs.Count * ButtonHeight;

		Rect scrollViewPosition = new Rect(x: 0f, y: DrawYPos, width: windowWidth, height: windowHeight);
		Rect scrollViewViewRect = new Rect(x: 0f, y: 0f, width: viewWidth, height: viewHeight);

		GUI.DrawTexture(scrollViewPosition, BluConsoleSkin.EvenBackTexture);

		Vector2 oldScrollPosition = _logListScrollPosition;
		_logListScrollPosition = GUI.BeginScrollView(position: scrollViewPosition,
		                                             scrollPosition: _logListScrollPosition,
		                                             viewRect: scrollViewViewRect);

		int firstRenderLogIndex = (int)(_logListScrollPosition.y / ButtonHeight);
		firstRenderLogIndex = Mathf.Clamp(firstRenderLogIndex, 0, logs.Count);

		int lastRenderLogIndex = firstRenderLogIndex + (int)(windowHeight / ButtonHeight) + 2;
		lastRenderLogIndex = Mathf.Clamp(lastRenderLogIndex, 0, logs.Count);

		float buttonY = firstRenderLogIndex * ButtonHeight;
		bool hasSomeClick = false;

		for (int i = firstRenderLogIndex; i < lastRenderLogIndex; i++)
		{
			LogInfo logInfo = logs[i].Log;
			var styleBack = GetLogBackStyle(i, logInfo);

			var styleMessage = GetLogListStyle(i, logInfo);
			string showMessage = GetTruncatedMessage(GetLogListMessage(logInfo));
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

			if (_isCollapse)
			{
				int quantity = logs[i].Quantity;
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
				hasSomeClick = true;

				if (!isLeftClick && i == _logListSelectedMessage)
					DrawPopup(Event.current, logInfo);

				if (isLeftClick && i == _logListSelectedMessage)
				{
					if (IsDoubleClickLogListButton)
					{
						_logListLastTimeClicked = 0.0f;
						if (logInfo.CallStack.Count > 0)
							JumpToSource(logInfo.CallStack[0]);
					}
					else
					{
						_logListLastTimeClicked = EditorApplication.timeSinceStartup;
					}
				}
				else
				{
					_logListSelectedMessage = i;
				}

				_logDetailSelectedFrame = -1;
			}

			buttonY += ButtonHeight;
		}

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
			_topPanelHeight = Event.current.mousePosition.y;
			_cursorChangeRect.Set(_cursorChangeRect.x, resizerY, _cursorChangeRect.width, _cursorChangeRect.height);
		}

		_topPanelHeight = Mathf.Clamp(_topPanelHeight,
		                              MinHeightOfTopAndBottom,
		                              position.height - MinHeightOfTopAndBottom);
	}
		
	private void DrawLogDetail()
	{
		List<CountedLog> logs = CountedLogsFilteredByFlags;
		var windowHeight = WindowHeight - DrawYPos;

		{
			var rect = new Rect(x: 0, y: DrawYPos, width: WindowWidth, height: windowHeight);
			GUI.DrawTexture(rect, BluConsoleSkin.EvenBackTexture);
		}

		if (_logListSelectedMessage == -1 || logs.Count == 0 || _logListSelectedMessage >= logs.Count)
			return;

		var log = logs[_logListSelectedMessage].Log;
		var size = log.CallStack.Count;
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
		if (_logListSelectedMessage == -1 || logs.Count == 0 || _logListSelectedMessage >= logs.Count)
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
				bool isLeftClick = Event.current.button == 0;

				if (!isLeftClick && _logDetailSelectedFrame == -2)
					DrawPopup(Event.current, log);

				if (isLeftClick && _logDetailSelectedFrame == -2)
				{
					if (IsDoubleClickLogDetailButton)
					{
						_logDetailLastTimeClicked = 0.0f;
						if (log.CallStack.Count > 0)
							JumpToSource(log.CallStack[0]);
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
			var frame = log.CallStack[i];
			var contentMessage = new GUIContent(GetTruncatedMessage(frame.FormattedMethodName));

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
				bool isLeftClick = Event.current.button == 0;

				if (isLeftClick && i == _logDetailSelectedFrame)
				{
					if (IsDoubleClickLogDetailButton)
					{
						_logDetailLastTimeClicked = 0.0f;
						JumpToSource(frame);
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
		LogInfo log)
	{
		GenericMenu.MenuFunction copyCallback = () => {
			EditorGUIUtility.systemCopyBuffer = log.Message;
		};

		GenericMenu menu = new GenericMenu();
		menu.AddItem(content: new GUIContent("Copy"), on: false, func: copyCallback);
		menu.ShowAsContext();

		clickEvent.Use();
	}

	private void SetCountedLogsDirty()
	{
		_isCountedLogsDirty = true;
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

	#region Gets

	private float GetMaxDetailMessageHeight(float normalHeight)
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
		LogInfo log)
	{
		return log.Message.Replace(System.Environment.NewLine, " ");
	}

	private GUIStyle GetLogBackStyle(
		int index,
		LogInfo log)
	{
        return index % 2 == 0 ? BluConsoleSkin.EvenBackStyle : BluConsoleSkin.OddBackStyle;
	}

	private GUIStyle GetLogListStyle(
		int index,
		LogInfo log)
	{
		return GetLogListStyle(log.LogType);
	}

	private GUIStyle GetLogListStyle(
		BluLogType logType)
	{
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

	private bool CanLog(
		LogInfo log)
	{
		return (((int)log.LogType) & LogTypeMask) != 0;
	}

	private void JumpToSource(
		LogStackFrame frame)
	{
		if (String.IsNullOrEmpty(frame.FilePath))
			return;
		
		var filename = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), frame.FilePath);
		if (System.IO.File.Exists(filename))
		{
			UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(frame.FilePath, frame.Line);
		}
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

	// TODO: The two below functions are equal... Refactor that. DRY!
	private List<CountedLog> FilterByPatternHelmLike(
		string pattern,
		List<CountedLog> logs)
	{
		if (string.IsNullOrEmpty(pattern))
			return logs;

		string[] patterns = pattern.ToLower().Split(' ');

		List<CountedLog> logsFiltered = new List<CountedLog>(logs.Count);
		for (int i = 0; i < logs.Count; i++)
		{
			CountedLog log = logs[i];
			var messageLower = log.Log.Message.ToLower();

			bool hasPattern = true;
			for (int j = 0; j < patterns.Length; j++)
			{
				if (!messageLower.Contains(patterns[j]))
				{
					hasPattern = false;
					break;
				}
			}
			if (hasPattern)
				logsFiltered.Add(log);
		}

		return logsFiltered;
	}

	private List<LogInfo> FilterByPatternHelmLike(
		string pattern,
		List<LogInfo> logs)
	{
		if (string.IsNullOrEmpty(pattern))
			return logs;

		string[] patterns = pattern.ToLower().Split(' ');

		List<LogInfo> logsFiltered = new List<LogInfo>(logs.Count);
		for (int i = 0; i < logs.Count; i++)
		{
			LogInfo log = logs[i];
			var messageLower = log.Message.ToLower();

			bool hasPattern = true;
			for (int j = 0; j < patterns.Length; j++)
			{
				if (!messageLower.Contains(patterns[j]))
				{
					hasPattern = false;
					break;
				}
			}
			if (hasPattern)
				logsFiltered.Add(log);
		}

		return logsFiltered;
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

	private List<CountedLog> CountedLogsFilteredByFlags
	{
		get
		{
			if (!_isCountedLogsDirty)
				return _countedLogs;

			List<CountedLog> logsFiltered;

			if (_isCollapse)
			{
				List<CountedLog> logs = FilterByPatternHelmLike(_searchString, _loggerAsset.CountedLogs);
				logsFiltered = new List<CountedLog>(logs.Count);
				for (int i = 0; i < logs.Count; i++)
				{
					CountedLog countedLog = logs[i];
					if (CanLog(countedLog.Log))
						logsFiltered.Add(countedLog);
				}

				_countedLogs = logsFiltered;
			}
			else
			{
				List<LogInfo> logs = FilterByPatternHelmLike(_searchString, _loggerAsset.LogsInfo);
				logsFiltered = new List<CountedLog>(logs.Count);
				for (int i = 0; i < logs.Count; i++)
				{
					LogInfo log = logs[i];
					if (CanLog(log))
						logsFiltered.Add(new CountedLog(log, 1));
				}

				_countedLogs = logsFiltered;
			}

			_isCountedLogsDirty = false;

			return logsFiltered;
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
