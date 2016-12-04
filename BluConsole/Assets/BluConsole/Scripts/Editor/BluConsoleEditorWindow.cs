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
using System.Linq;
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

	private Texture2D _selectedBackTexture;
	private Texture2D _oddErrorBackTexture;
	private Texture2D _evenErrorBackTexture;
	private Texture2D _sizeLinerBorderTexture;
	private Texture2D _sizeLinerCenterTexture;

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

	// Compiling
	private bool _isCompiling = false;
	private bool _isPlaying = false;
	private List<LogInfo> _dirtyLogsBeforeCompile = new List<LogInfo>();

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

	private void OnGUI()
	{
		_loggerAsset.OnNewLogOrTrimLogEvent -= OnNewLogOrTrimLog;
		_loggerAsset.OnNewLogOrTrimLogEvent += OnNewLogOrTrimLog;

		if (EditorApplication.isCompiling && !_isCompiling)
		{
			_isCompiling = true;
			OnBeforeCompile();
		}
		else if (!EditorApplication.isCompiling && _isCompiling)
		{
			_isCompiling = false;
			OnAfterCompile();
		}

		if (EditorApplication.isPlaying && !_isPlaying)
		{
			_isPlaying = true;
			OnBeginPlay();
		}
		else if (!EditorApplication.isPlaying && _isPlaying)
		{
			_isPlaying = false;
		}

		InitVariables();

		_hasScrollWheelUp = Event.current.type == EventType.ScrollWheel && Event.current.delta.y < 0f;

		DrawResizer();

		GUILayout.BeginVertical(GUILayout.Height(_topPanelHeight), GUILayout.MinHeight(MinHeightOfTopAndBottom));

		_drawYPos += DrawTopToolbar();
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

	private void OnBeforeCompile()
	{
		SetCountedLogsDirty();

		_dirtyLogsBeforeCompile = new List<LogInfo>(_loggerAsset.LogsInfo.Where(
				log => log.IsCompileMessage));
	}

	private void OnAfterCompile()
	{
		SetCountedLogsDirty();

		var logsBlackList = new HashSet<LogInfo>(_dirtyLogsBeforeCompile, new LogInfoComparer());
		_loggerAsset.Clear(
				log => logsBlackList.Contains(log));
	}

	private void OnBeginPlay()
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

	private void OnDisable()
	{
		Texture2D.DestroyImmediate(_selectedBackTexture);
		Texture2D.DestroyImmediate(_oddErrorBackTexture);
		Texture2D.DestroyImmediate(_evenErrorBackTexture);
		Texture2D.DestroyImmediate(_sizeLinerBorderTexture);
		Texture2D.DestroyImmediate(_sizeLinerCenterTexture);
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
		int qtNormalLogs = 0, qtWarningLogs = 0, qtErrorLogs = 0;
		var logs = CountedLogsFilteredByFlags;
		for (int i = 0; i < logs.Count; i++)
		{
			switch (logs[i].Log.LogType)
			{
			case BluLogType.Normal:
				qtNormalLogs++;
				break;
			case BluLogType.Warning:
				qtWarningLogs++;
				break;
			case BluLogType.Error:
				qtErrorLogs++;
				break;
			}
		}

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
		_isShowErrors =
			GetToggleClamped(_isShowErrors, 
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
			var styleBack = GetLogBackStyle(i, logInfo, i == _logListSelectedMessage ? true : false);


			var styleImage = GetLogStyle(logInfo.LogType);
			var contentImage = new GUIContent(GetIcon(logInfo.LogType));
			var contentImageWidth = styleImage.CalcSize(contentImage).x;
			var rectImage = new Rect(x: 0,
			                         y: buttonY,
			                         width: contentImageWidth,
			                         height: ButtonHeight);
			DrawBack(rectImage, styleBack);
			bool imageClicked = GUI.Button(rectImage, contentImage, styleImage);


			var styleMessage = BluConsoleSkin.MessageStyle;
			string showMessage = GetTruncatedMessage(GetLogListMessage(logInfo));
			var contentMessage = new GUIContent(showMessage);
			var rectMessage = new Rect(x: contentImageWidth,
			                           y: buttonY,
			                           width: viewWidth - contentImageWidth,
			                           height: ButtonHeight);
			DrawBack(rectMessage, styleBack);
			bool messageClicked = GUI.Button(rectMessage, contentMessage, styleMessage);


			bool hasClick = messageClicked || imageClicked;
			bool isLeftClick = hasClick ? Event.current.button == 0 : false;

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

			if (hasClick)
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

		// Getting maximum width
		float viewWidth = WindowWidth;
		viewWidth = Mathf.Max(viewWidth, GetButtonWidth(GetTruncatedMessage(log.Message), log.LogType));
		for (int i = 0; i < size; i++)
		{
			var frame = log.CallStack[i];
			var message = frame.FormattedMethodName;
			message = GetTruncatedMessage(message);
			viewWidth = Mathf.Max(viewWidth, GetButtonWidth(message, log.LogType));
		}

		float viewHeight = sizePlus * ButtonHeight;

		Rect scrollViewPosition = new Rect(x: 0f, y: DrawYPos, width: WindowWidth, height: windowHeight);
		Rect scrollViewViewRect = new Rect(x: 0f, y: 0f, width: viewWidth, height: viewHeight);

		_logDetailScrollPosition = GUI.BeginScrollView(position: scrollViewPosition,
		                                               scrollPosition: _logDetailScrollPosition,
		                                               viewRect: scrollViewViewRect);

		int firstRenderLogIndex = (int)(_logDetailScrollPosition.y / ButtonHeight);
		firstRenderLogIndex = Mathf.Clamp(firstRenderLogIndex, 0, sizePlus);

		int lastRenderLogIndex = firstRenderLogIndex + (int)(windowHeight / ButtonHeight) + 2;
		lastRenderLogIndex = Mathf.Clamp(lastRenderLogIndex, 0, sizePlus);

		float buttonY = firstRenderLogIndex * ButtonHeight;

		// Return if doesn't has nothing to show
		if (_logListSelectedMessage == -1 || logs.Count == 0 || _logListSelectedMessage >= logs.Count)
		{
			GUI.EndScrollView();
			return;
		}

		// Logging first message
		if (firstRenderLogIndex == 0)
		{
			var styleBack = GetLogBackStyle(0, log, _logDetailSelectedFrame == -2 ? true : false);
			var styleMessage = BluConsoleSkin.MessageStyle;
			var rectButton = new Rect(x: 0, y: buttonY, width: viewWidth, height: ButtonHeight);

			var contentMessage = new GUIContent(GetTruncatedMessage(log.Message));

			DrawBack(rectButton, styleBack);
			if (GUI.Button(rectButton, contentMessage, styleMessage))
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

			buttonY += ButtonHeight;
		}

		for (int i = firstRenderLogIndex == 0 ? 0 : firstRenderLogIndex - 1; i + 1 < lastRenderLogIndex; i++)
		{
			var frame = log.CallStack[i];
			var contentMessage = new GUIContent(GetTruncatedMessage(frame.FormattedMethodName));

			var styleBack = GetLogBackStyle(0, log, i == _logDetailSelectedFrame ? true : false);
			var styleMessage = BluConsoleSkin.MessageStyle;
			var rectButton = new Rect(x: 0, y: buttonY, width: viewWidth, height: ButtonHeight);

			DrawBack(rectButton, styleBack);
			if (GUI.Button(rectButton, contentMessage, styleMessage))
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

			buttonY += ButtonHeight;
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
		GUIStyle style)
	{
		GUI.Label(rect, GUIContent.none, style);
	}


	#region Gets


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
		LogInfo log,
		bool selected)
	{
		if (selected)
			return SelectedBackStyle;

		bool isEven = index % 2 == 0 ? true : false;
		bool isCompileError = log.IsCompileMessage && log.LogType == BluLogType.Error;

		if (isEven)
		{
			if (isCompileError)
				return EvenErrorBackStyle;
			else
				return BluConsoleSkin.EvenBackStyle;
		}
		else
		{
			if (isCompileError)
				return OddErrorBackStyle;
			else
				return BluConsoleSkin.OddBackStyle;
		}
	}

	private GUIStyle GetLogStyle(
		int index,
		LogInfo log)
	{
		if (index == _logListSelectedMessage)
			return SelectedBackStyle;

		return GetLogStyle(log.LogType);
	}

	private GUIStyle GetLogStyle(
		BluLogType logType)
	{
		switch (logType)
		{
		case BluLogType.Normal:
			return BluConsoleSkin.LogImageInfoStyle;
		case BluLogType.Warning:
			return BluConsoleSkin.LogImageWarnStyle;
		case BluLogType.Error:
			return BluConsoleSkin.LogImageErrorStyle;
		}
		return BluConsoleSkin.LogImageInfoStyle;
	}

	private float GetButtonWidth(
		string message,
		BluLogType logType)
	{
		return BluConsoleSkin.MessageStyle.CalcSize(new GUIContent(message, GetIcon(logType))).x;
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


	private Texture2D SizeLinerCenterTexture
	{
		get
		{
			if (_sizeLinerCenterTexture == null)
				_sizeLinerCenterTexture = BluConsoleEditorHelper.GetTexture(BluConsoleSkin.SizerLineCenterColour);
			return _sizeLinerCenterTexture;
		}
	}

	private Texture2D SizeLinerBorderTexture
	{
		get
		{
			if (_sizeLinerBorderTexture == null)
				_sizeLinerBorderTexture = BluConsoleEditorHelper.GetTexture(BluConsoleSkin.SizerLineBorderColour);
			return _sizeLinerBorderTexture;
		}
	}

	private Texture2D EvenErrorBackTexture
	{
		get
		{
			if (_evenErrorBackTexture == null)
				_evenErrorBackTexture = BluConsoleEditorHelper.GetTexture(BluConsoleSkin.EvenErrorBackColor);
			return _evenErrorBackTexture;
		}
	}

	private Texture2D OddErrorBackTexture
	{
		get
		{
			if (_oddErrorBackTexture == null)
				_oddErrorBackTexture = BluConsoleEditorHelper.GetTexture(BluConsoleSkin.OddErrorBackColor);
			return _oddErrorBackTexture;
		}
	}

	private Texture2D SelectedBackTexture
	{
		get
		{
			if (_selectedBackTexture == null)
				_selectedBackTexture = BluConsoleEditorHelper.GetTexture(BluConsoleSkin.SelectedBackColor);
			return _selectedBackTexture;
		}
	}

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

	private GUIStyle EvenErrorBackStyle
	{
		get
		{
			var style = new GUIStyle(BluConsoleSkin.EvenBackStyle);
			style.normal.background = EvenErrorBackTexture;
			return style;
		}
	}

	private GUIStyle OddErrorBackStyle
	{
		get
		{
			var style = new GUIStyle(BluConsoleSkin.OddBackStyle);
			style.normal.background = OddErrorBackTexture;
			return style;
		}
	}

	public GUIStyle SelectedBackStyle
	{
		get
		{
			var style = new GUIStyle(BluConsoleSkin.EvenBackStyle);
			style.normal.background = SelectedBackTexture;
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


	#endregion Properties

}

}
