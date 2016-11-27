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
	private List<CountedLog> _countedLogs = new List<CountedLog>();
	private bool _isCountedLogsDirty = true;

	private GUIStyle _evenButtonStyle;
	private GUIStyle _oddButtonStyle;
	private GUIStyle _evenErrorButtonStyle;
	private GUIStyle _oddButtonErrorStyle;
	private GUIStyle _selectedButtonStyle;
	private GUIStyle _collapseStyle;

	private Texture2D _selectedButtonTexture;
	private Texture2D _oddErrorButtonTexture;
	private Texture2D _evenErrorButtonTexture;
	private Texture2D _oddButtonTexture;
	private Texture2D _evenButtonTexture;
	private Texture2D _sizeLinerBorderTexture;
	private Texture2D _sizeLinerCenterTexture;

	private float _buttonWidth;
	private float _buttonHeight;

    private Color _guiColor;

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

		var consoleIcon = BluConsoleEditorHelper.ConsoleIcon;
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
        _guiColor = GUI.backgroundColor;

		_evenButtonStyle = new GUIStyle(EditorStyles.label);
		_evenButtonStyle.richText = true;
		_evenButtonStyle.fontSize = FontSize;
		_evenButtonStyle.alignment = TextAnchor.MiddleLeft;
		_evenButtonStyle.margin = new RectOffset(0, 0, 0, 0);
		_evenButtonStyle.padding = new RectOffset(0, 0, 0, 0);
		_evenButtonStyle.normal.background = EvenButtonTexture;
		_evenButtonStyle.active = _evenButtonStyle.normal;
		_evenButtonStyle.hover = _evenButtonStyle.normal;
		_evenButtonStyle.focused = _evenButtonStyle.normal;

		_oddButtonStyle = new GUIStyle(EditorStyles.label);
		_oddButtonStyle.richText = true;
		_oddButtonStyle.fontSize = FontSize;
		_oddButtonStyle.alignment = TextAnchor.MiddleLeft;
		_oddButtonStyle.margin = new RectOffset(0, 0, 0, 0);
		_oddButtonStyle.padding = new RectOffset(0, 0, 0, 0);
		_oddButtonStyle.normal.background = OddButtonTexture;
		_oddButtonStyle.active = _oddButtonStyle.normal;
		_oddButtonStyle.hover = _oddButtonStyle.normal;
		_oddButtonStyle.focused = _oddButtonStyle.normal;

		_evenErrorButtonStyle = new GUIStyle(EditorStyles.label);
		_evenErrorButtonStyle.richText = true;
		_evenErrorButtonStyle.fontSize = FontSize;
		_evenErrorButtonStyle.alignment = TextAnchor.MiddleLeft;
		_evenErrorButtonStyle.margin = new RectOffset(0, 0, 0, 0);
		_evenErrorButtonStyle.padding = new RectOffset(0, 0, 0, 0);
		_evenErrorButtonStyle.normal.background = EvenErrorButtonTexture;
		_evenErrorButtonStyle.active = _evenErrorButtonStyle.normal;
		_evenErrorButtonStyle.hover = _evenErrorButtonStyle.normal;
		_evenErrorButtonStyle.focused = _evenErrorButtonStyle.normal;

		_oddButtonErrorStyle = new GUIStyle(EditorStyles.label);
		_oddButtonErrorStyle.richText = true;
		_oddButtonErrorStyle.fontSize = FontSize;
		_oddButtonErrorStyle.alignment = TextAnchor.MiddleLeft;
		_oddButtonErrorStyle.margin = new RectOffset(0, 0, 0, 0);
		_oddButtonErrorStyle.padding = new RectOffset(0, 0, 0, 0);
		_oddButtonErrorStyle.normal.background = OddErrorButtonTexture;
		_oddButtonErrorStyle.active = _oddButtonErrorStyle.normal;
		_oddButtonErrorStyle.hover = _oddButtonErrorStyle.normal;
		_oddButtonErrorStyle.focused = _oddButtonErrorStyle.normal;

		_selectedButtonStyle = new GUIStyle(EditorStyles.whiteLabel);
		_selectedButtonStyle.richText = true;
		_selectedButtonStyle.fontSize = FontSize;
		_selectedButtonStyle.alignment = TextAnchor.MiddleLeft;
		_selectedButtonStyle.margin = new RectOffset(0, 0, 0, 0);
		_selectedButtonStyle.padding = new RectOffset(0, 0, 0, 0);
		_selectedButtonStyle.normal.background = SelectedButtonTexture;
		_selectedButtonStyle.active = _selectedButtonStyle.normal;
		_selectedButtonStyle.hover = _selectedButtonStyle.normal;
		_selectedButtonStyle.focused = _selectedButtonStyle.normal;

		foreach (var style in GUI.skin.customStyles)
		{
			if (style.name == "CN CountBadge")
			{
				_collapseStyle = style;
				break;
			}
		}

		if (_collapseStyle == null)
			_collapseStyle = EvenButtonStyle;

		_buttonWidth = position.width;
		_buttonHeight = _evenButtonStyle.CalcSize(new GUIContent("Test")).y + 15.0f;
		_drawYPos = 0f;
	}

    private void ClearVariables()
    {
        Texture2D.DestroyImmediate(_evenButtonTexture, true);
        Texture2D.DestroyImmediate(_oddButtonTexture, true);
        Texture2D.DestroyImmediate(_evenErrorButtonTexture, true);
        Texture2D.DestroyImmediate(_oddErrorButtonTexture, true);
        Texture2D.DestroyImmediate(_sizeLinerBorderTexture, true);
        Texture2D.DestroyImmediate(_sizeLinerCenterTexture, true);
        Texture2D.DestroyImmediate(_selectedButtonTexture, true);
    }

	private void OnBeforeCompile()
	{
		SetCountedLogsDirty();

		_dirtyLogsBeforeCompile = new List<LogInfo>(_loggerAsset.LogsInfo.Where(
				log => log.IsCompileMessage));
	}

	private void OnAfterCompile()
	{
        ClearVariables();
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

	private void OnNewLogOrTrimLog()
	{
		SetCountedLogsDirty();
	}

	private float DrawTopToolbar()
	{
		float height = EditorStyles.toolbarButton.CalcSize(new GUIContent("Clear")).y;
		GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(height));

		// Clear/Collapse/ClearOnPlay/ErrorPause Area
		if (BluConsoleEditorHelper.ButtonClamped("Clear", EditorStyles.toolbarButton))
		{
			SetCountedLogsDirty();
			_loggerAsset.ClearExceptCompileErrors();
			_logListSelectedMessage = -1;
			_logDetailSelectedFrame = -1;
		}

		GUILayout.Space(6.0f);

		bool oldCollapseValue = _isCollapse;
		_isCollapse = BluConsoleEditorHelper.ToggleClamped(_isCollapse,
		                                                   "Collapse",
		                                                   EditorStyles.toolbarButton);
		if (oldCollapseValue != _isCollapse)
			SetCountedLogsDirty();

		_isClearOnPlay = BluConsoleEditorHelper.ToggleClamped(_isClearOnPlay,
		                                                      "Clear on Play",
		                                                      EditorStyles.toolbarButton);

		_loggerAsset.IsPauseOnError = BluConsoleEditorHelper.ToggleClamped(_loggerAsset.IsPauseOnError,
		                                                                   "Pause on Error",
		                                                                   EditorStyles.toolbarButton);


		GUILayout.FlexibleSpace();


		// Search Area
		string oldSearchString = _searchString;
		_searchString = EditorGUILayout.TextArea(_searchString,
		                                         BluConsoleEditorHelper.ToolbarSearchTextField,
		                                         GUILayout.Width(200.0f));
		if (oldSearchString != _searchString)
			SetCountedLogsDirty();

		if (GUILayout.Button("", BluConsoleEditorHelper.ToolbarSearchCancelButtonStyle))
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
		_isShowNormal =
            BluConsoleEditorHelper.ToggleClamped(_isShowNormal,
		                                         BluConsoleEditorHelper.InfoGUIContent(qtNormalLogsStr),
		                                         EditorStyles.toolbarButton);
		if (oldIsShowNormal != _isShowNormal)
			SetCountedLogsDirty();


		bool oldIsShowWarnings = _isShowWarnings;
		_isShowWarnings =
            BluConsoleEditorHelper.ToggleClamped(_isShowWarnings,
		                                         BluConsoleEditorHelper.WarningGUIContent(qtWarningLogsStr),
		                                         EditorStyles.toolbarButton);
		if (oldIsShowWarnings != _isShowWarnings)
			SetCountedLogsDirty();


		bool oldIsShowErrors = _isShowErrors;
		_isShowErrors =
            BluConsoleEditorHelper.ToggleClamped(_isShowErrors,
		                                         BluConsoleEditorHelper.ErrorGUIContent(qtErrorLogsStr),
		                                         EditorStyles.toolbarButton);
		if (oldIsShowErrors != _isShowErrors)
			SetCountedLogsDirty();


		GUILayout.EndHorizontal();

		return height;
	}


	#region LogList


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

		GUI.DrawTexture(scrollViewPosition, EvenButtonTexture);

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
			int quantity = logs[i].Quantity;

			var style = GetLogListStyle(i, logInfo);
			string showMessage = GetTruncatedMessage(GetLogListMessage(logInfo));
			var content = new GUIContent(showMessage);
			var contentImage = new GUIContent(GetIcon(logInfo.LogType));
			var contentImageWidth = style.CalcSize(contentImage).x;

			var buttonRect = new Rect(x: contentImageWidth,
			                          y: buttonY,
			                          width: viewWidth - contentImageWidth,
			                          height: ButtonHeight);
			var imageRect = new Rect(x: 0,
			                         y: buttonY,
			                         width: contentImageWidth,
			                         height: ButtonHeight);

			bool buttonClicked = GUI.Button(buttonRect, content, style);
			bool imageClicked = GUI.Button(imageRect, contentImage, style);
			bool hasClick = buttonClicked || imageClicked;
			bool isLeftClick = hasClick ? Event.current.button == 0: false;

			if (_isCollapse)
			{
				var collapseCount = Mathf.Min(quantity, MAX_LENGTH_COLLAPSE);
				var collapseText = collapseCount.ToString();
				if (collapseCount >= MAX_LENGTH_COLLAPSE)
					collapseText += "+";
				var collapseContent = new GUIContent(collapseText);
				var collapseSize = CollapseStyle.CalcSize(collapseContent);

				var collapseRect = new Rect(x: viewWidth - collapseSize.x - 5f,
				                            y: (buttonY + buttonY + ButtonHeight - collapseSize.y) * 0.5f,
				                            width: collapseSize.x,
				                            height: collapseSize.y);

				GUI.Label(collapseRect, collapseContent, _collapseStyle);
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
						{
							JumpToSource(logInfo.CallStack[0]);
						}
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

	private string GetLogListMessage(
		LogInfo log)
	{
		string showMessage = null;
		if (log.IsCompileMessage)
			showMessage = log.RawMessage;
		else
			showMessage = log.Message.Replace(System.Environment.NewLine, " ");
		return showMessage;
	}

	private GUIStyle GetLogListStyle(
		int index,
		LogInfo log)
	{
		bool isEven = index % 2 == 0 ? true : false;

		var logLineStyle = EvenButtonStyle;
		if (log.IsCompileMessage && log.LogType == BluLogType.Error && index != _logListSelectedMessage)
		{
			logLineStyle = isEven ? EvenErrorButtonStyle : OddButtonErrorStyle;
		}
		else
		{
			logLineStyle = index == _logListSelectedMessage ?
                SelectedButtonStyle : (isEven ? EvenButtonStyle : OddButtonStyle);
		}

		return logLineStyle;
	}


	#endregion LogList


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
			GUI.DrawTexture(rect, EvenButtonTexture);
		}

		if (_logListSelectedMessage == -1 || logs.Count == 0 || _logListSelectedMessage >= logs.Count)
		{
			return;
		}

		var log = logs[_logListSelectedMessage].Log;
		var size = log.CallStack.Count;
		var sizePlus = size + 1;

		// Getting maximum width
		float viewWidth = WindowWidth;
		if (log.IsCompileMessage)
			viewWidth = Mathf.Max(viewWidth, GetButtonWidth(GetTruncatedMessage(log.Message), log.LogType));
		else
			viewWidth = Mathf.Max(viewWidth, GetButtonWidth(GetTruncatedMessage(log.RawMessage), log.LogType));
		for (int i = 0; i < size; i++)
		{
			var frame = log.CallStack[i];
			var message = log.IsCompileMessage ? log.RawMessage : frame.FormattedMethodName;
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
			var style = _logDetailSelectedFrame == -2 ? SelectedButtonStyle : EvenButtonStyle;
			var buttonRect = new Rect(x: 0, y: buttonY, width: viewWidth, height: ButtonHeight);

			var message = log.IsCompileMessage ? log.Message : log.RawMessage;
			message = GetTruncatedMessage(message);
			message = "  " + message;
			var content = new GUIContent(message);

			if (GUI.Button(buttonRect, content, style))
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
			var message = log.IsCompileMessage ? log.RawMessage : frame.FormattedMethodName;
			message = GetTruncatedMessage(message);
			message = "  " + message;
			var content = new GUIContent(message);

			var style = i == _logDetailSelectedFrame ? SelectedButtonStyle : (i % 2 == 0 ? EvenButtonStyle : OddButtonStyle);
			var buttonRect = new Rect(x: 0, y: buttonY, width: viewWidth, height: ButtonHeight);

			if (GUI.Button(buttonRect, content, style))
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
			EditorGUIUtility.systemCopyBuffer = log.IsCompileMessage ? log.RawMessage : log.Message;
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


	#region Gets


	private string GetTruncatedMessage(
		string message)
	{
		if (message.Length <= MAX_LENGTH_MESSAGE)
			return message;

		return string.Format("{0}... <truncated>", message.Substring(startIndex: 0, length: MAX_LENGTH_MESSAGE));
	}

	private float GetButtonWidth(
		string message,
		BluLogType logType)
	{
		return EvenButtonStyle.CalcSize(new GUIContent(message, GetIcon(logType))).x;
	}

	private Texture2D GetIcon(
		BluLogType logType)
	{
		switch (logType)
		{
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

	private Texture2D GetIconSmall(
		BluLogType logType)
	{
		switch (logType)
		{
		case BluLogType.Normal:
			return BluConsoleEditorHelper.InfoIconSmall;
		case BluLogType.Warning:
			return BluConsoleEditorHelper.WarningIconSmall;
		case BluLogType.Error:
			return BluConsoleEditorHelper.ErrorIconSmall;
		default:
			return BluConsoleEditorHelper.InfoIconSmall;
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
		var filename = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), frame.FileRelativePath);
		if (System.IO.File.Exists(filename))
		{
			UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(frame.FileRelativePath, frame.Line);
		}
	}


	#endregion Gets


	#region Properties


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
            return BluConsoleEditorHelper.ColorPercent(_guiColor, 0.88f);
		}
	}

	private Color OddButtonColor
	{
		get
		{
            return BluConsoleEditorHelper.ColorPercent(_guiColor, 0.85f);
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

	private Color SelectedButtonColor
	{
		get
		{
			return new Color(0.5f, 0.5f, 1);
		}
	}

	private Texture2D SizeLinerCenterTexture
	{
		get
		{
			if (_sizeLinerCenterTexture == null)
				_sizeLinerCenterTexture = BluConsoleEditorHelper.GetTexture(SizerLineCenterColour);
			return _sizeLinerCenterTexture;
		}
	}

	private Texture2D SizeLinerBorderTexture
	{
		get
		{
			if (_sizeLinerBorderTexture == null)
				_sizeLinerBorderTexture = BluConsoleEditorHelper.GetTexture(SizerLineBorderColour);
			return _sizeLinerBorderTexture;
		}
	}

	private Texture2D EvenButtonTexture
	{
		get
		{
			if (_evenButtonTexture == null)
				_evenButtonTexture = BluConsoleEditorHelper.GetTexture(EvenButtonColor);
			return _evenButtonTexture;
		}
	}

	private Texture2D OddButtonTexture
	{
		get
		{
			if (_oddButtonTexture == null)
				_oddButtonTexture = BluConsoleEditorHelper.GetTexture(OddButtonColor);
			return _oddButtonTexture;
		}
	}

	private Texture2D EvenErrorButtonTexture
	{
		get
		{
			if (_evenErrorButtonTexture == null)
				_evenErrorButtonTexture = BluConsoleEditorHelper.GetTexture(EvenErrorButtonColor);
			return _evenErrorButtonTexture;
		}
	}

	private Texture2D OddErrorButtonTexture
	{
		get
		{
			if (_oddErrorButtonTexture == null)
				_oddErrorButtonTexture = BluConsoleEditorHelper.GetTexture(OddErrorButtonColor);
			return _oddErrorButtonTexture;
		}
	}

	private Texture2D SelectedButtonTexture
	{
		get
		{
			if (_selectedButtonTexture == null)
				_selectedButtonTexture = BluConsoleEditorHelper.GetTexture(SelectedButtonColor);
			return _selectedButtonTexture;
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

	private GUIStyle CollapseStyle
	{
		get
		{
			return _collapseStyle;
		}
	}


	#endregion Properties

}

}
