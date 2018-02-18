using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BluConsole.Extensions;
using BluConsole.Core;
using BluConsole.Core.UnityLoggerApi;


namespace BluConsole.Editor
{

	[Serializable]
	public class BluDetailWindow : BluConsoleWindow
	{

		public int ListWindowSelectedMessage { get; set; }

		private BluLog _selectedLog;

		public override void OnGUI(int id)
		{
			base.OnGUI(id);

            UnityLoggerServer.StartGettingLogs();

			GUI.DrawTexture(WindowRect, BluConsoleSkin.EvenBackTexture);

            if (ListWindowSelectedMessage != -1 &&
                ListWindowSelectedMessage >= 0 &&
                ListWindowSelectedMessage < QtLogs)
            {
                _selectedLog = GetCompleteLog(Rows[ListWindowSelectedMessage]);
            }

            if (_selectedLog != null)
				SelectedMessage = Mathf.Clamp(SelectedMessage, 0, _selectedLog.StackTrace.Count);
			else
				SelectedMessage = -1;

            if (ListWindowSelectedMessage == -1 ||
                QtLogs == 0 ||
                ListWindowSelectedMessage >= QtLogs ||
                _selectedLog == null ||
                _selectedLog.StackTrace == null)
            {
                UnityLoggerServer.StopGettingsLogs();
                return;
            }

            var log = _selectedLog;
            var size = log.StackTrace.Count;
            var sizePlus = size + 1;

            float buttonHeight = GetDetailMessageHeight("A", BluConsoleSkin.MessageDetailCallstackStyle);
            float buttonWidth = DefaultButtonWidth;
            float firstLogHeight = Mathf.Max(buttonHeight, GetDetailMessageHeight(GetTruncatedMessage(log),
                                                                                  BluConsoleSkin.MessageDetailFirstLogStyle,
                                                                                  buttonWidth));

            float viewHeight = size * buttonHeight + firstLogHeight;

            if (viewHeight > WindowRect.height)
            {
                buttonWidth -= 15f;

                // Recalculate it because we decreased the buttonWidth
                firstLogHeight = Mathf.Max(buttonHeight, 
                                           GetDetailMessageHeight(GetTruncatedMessage(log),
                                                                  BluConsoleSkin.MessageDetailFirstLogStyle,
                                                                  buttonWidth));
            }
            viewHeight = size * buttonHeight + firstLogHeight;

            float viewWidth = buttonWidth;

            Rect scrollViewPosition = WindowRect;
            Rect scrollViewViewRect = new Rect(x: 0f, y: 0f, width: viewWidth, height: viewHeight);

            ScrollPosition = GUI.BeginScrollView(position: scrollViewPosition,
                                                 scrollPosition: ScrollPosition,
                                                 viewRect: scrollViewViewRect);

            // Return if has nothing to show
            if (ListWindowSelectedMessage == -1 || QtLogs == 0 || ListWindowSelectedMessage >= QtLogs)
            {
                GUI.EndScrollView();
                UnityLoggerServer.StopGettingsLogs();
                return;
            }

            float scrollY = ScrollPosition.y;

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
                if (WindowRect.height > offsetOfFirstLog)
                    lastRenderLogIndex = firstRenderLogIndex + (int)((WindowRect.height - offsetOfFirstLog) / buttonHeight) + 2;
                else
                    lastRenderLogIndex = 2;
            }
            else
            {
                lastRenderLogIndex = firstRenderLogIndex + (int)(WindowRect.height / buttonHeight) + 2;
            }
            lastRenderLogIndex = Mathf.Clamp(lastRenderLogIndex, 0, sizePlus);

            float buttonY = 0f;
            if (firstRenderLogIndex > 0)
                buttonY = firstLogHeight + (firstRenderLogIndex - 1) * buttonHeight;

            
            // Handling up/down arrow keys
            if (HasKeyboardArrowKeyInput)
            {
                int frame = SelectedMessage;
                bool isFrameOutsideOfRange = frame < firstRenderLogIndex + 1 || frame > lastRenderLogIndex - 2;
                if (isFrameOutsideOfRange && KeyboardArrowKeyDirection == Direction.Up)
                {
                    if (frame == 0)
                        ScrollPosition.y = 0f;
                    else
                        ScrollPosition.y = firstLogHeight + (frame - 1) * buttonHeight;
                }
                else if (isFrameOutsideOfRange && KeyboardArrowKeyDirection == Direction.Down)
                {
                    if (frame == 0)
                        ScrollPosition.y = 0f;
                    else
                        ScrollPosition.y = firstLogHeight + frame * buttonHeight - WindowRect.height;
                }
            }

            // Logging first message
            if (firstRenderLogIndex == 0)
            {
                var styleBack = BluConsoleSkin.GetLogBackStyle(0);
                var styleMessage = BluConsoleSkin.MessageDetailFirstLogStyle;
                var rectButton = new Rect(x: 0, y: buttonY, width: viewWidth, height: firstLogHeight);

                var isSelected = SelectedMessage == 0;
                var contentMessage = new GUIContent(GetTruncatedMessage(log));

                DrawBackground(rectButton, styleBack, isSelected);
                if (IsRepaintEvent)
                    styleMessage.Draw(rectButton, contentMessage, false, false, isSelected, false);

                bool messageClicked = IsClicked(rectButton);
                if (messageClicked)
                {
                    bool isLeftClick = Event.current.button == 0;
                    if (SelectedMessage != 0)
                        LastTimeClicked = 0.0f;
                    if (isLeftClick && !IsDoubleClick)
                        SelectedMessage = 0;
                    if (!isLeftClick)
                        DrawPopup(log);
                    if (isLeftClick && SelectedMessage == 0)
                    {
                        if (IsDoubleClick)
                        {
                            LastTimeClicked = 0.0f;
                            BluUtils.JumpToSourceFile(log, 0);
                        }
                        else
						{
                            LastTimeClicked = EditorApplication.timeSinceStartup;
						}
                    }
                }

                buttonY += firstLogHeight;
            }

            for (int i = firstRenderLogIndex == 0 ? 0 : firstRenderLogIndex - 1; i + 1 < lastRenderLogIndex; i++)
            {
                var contentMessage = new GUIContent(GetTruncatedMessage(log.StackTrace[i].FrameInformation));

                var styleBack = BluConsoleSkin.GetLogBackStyle(0);
                var styleMessage = BluConsoleSkin.MessageDetailCallstackStyle;
                var rectButton = new Rect(x: 0, y: buttonY, width: viewWidth, height: buttonHeight);

                var isSelected = i == (SelectedMessage - 1);
                DrawBackground(rectButton, styleBack, isSelected);
                if (IsRepaintEvent)
                    styleMessage.Draw(rectButton, contentMessage, false, false, isSelected, false);

                bool messageClicked = IsClicked(rectButton);
                if (messageClicked)
                {
                    bool isLeftClick = Event.current.button == 0;
                    if (!isSelected)
                        LastTimeClicked = 0.0f;
                    if (isLeftClick && !IsDoubleClick)
                        SelectedMessage = i+1;
                    if (isLeftClick && SelectedMessage-1 == i)
                    {
                        if (IsDoubleClick)
                        {
                            LastTimeClicked = 0.0f;
                            BluUtils.JumpToSourceFile(log, i);
                        }
                        else
						{
                            LastTimeClicked = EditorApplication.timeSinceStartup;
						}
                    }
                }

                buttonY += buttonHeight;
            }

            GUI.EndScrollView();

            UnityLoggerServer.StopGettingsLogs();
		}

        protected override void OnEnterKeyPressed()
        {
			if (_selectedLog == null)
				return;

            BluUtils.JumpToSourceFile(_selectedLog, SelectedMessage == 0 ? SelectedMessage : SelectedMessage-1);
        }	

		private float GetDetailMessageHeight(string message, GUIStyle style, float width = 0f)
		{
			return style.CalcHeight(new GUIContent(message), width);
		}

	}

}
