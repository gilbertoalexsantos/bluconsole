using System;
using System.Linq;
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

			GUI.DrawTexture(WindowRect, BluConsoleSkin.OddBackTexture);

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

            var cells = GetCells(_selectedLog);
            foreach (var cell in cells)
                cell.Message = GetTruncatedDetailMessage(cell.Message);

            float buttonWidth = DefaultButtonWidth;

            float viewHeight = cells.Sum(c => c.GetHeight(buttonWidth));
            if (viewHeight > WindowRect.height)
                buttonWidth -= 15f;
            viewHeight = cells.Sum(c => c.GetHeight(buttonWidth));

            float viewWidth = buttonWidth;

            Rect scrollViewPosition = WindowRect;
            Rect scrollViewViewRect = new Rect(x: 0f, y: 0f, width: viewWidth, height: viewHeight);

            ScrollPosition = GUI.BeginScrollView(position: scrollViewPosition,
                                                 scrollPosition: ScrollPosition,
                                                 viewRect: scrollViewViewRect);

            int firstRenderLogIndex = GetFirstGreaterCellIndex(cells, ScrollPosition.y, 0f, buttonWidth) - 1;
            firstRenderLogIndex = Mathf.Clamp(firstRenderLogIndex, 0, cells.Count - 1);
            int lastRenderLogIndex = GetFirstGreaterCellIndex(cells, ScrollPosition.y, WindowRect.height, buttonWidth);
            lastRenderLogIndex = Mathf.Clamp(lastRenderLogIndex, 0, cells.Count - 1);

            float buttonY = cells.Take(firstRenderLogIndex).Sum(c => c.GetHeight(buttonWidth));
            
            // Handling up/down arrow keys
            if (HasKeyboardArrowKeyInput)
            {
                float lo = ScrollPosition.y;
                float hi = ScrollPosition.y + WindowRect.height;
                float selectedMessageLo = cells.Take(SelectedMessage).Sum(c => c.GetHeight(buttonWidth));
                float selectedMessageHi = cells.Take(SelectedMessage+1).Sum(c => c.GetHeight(buttonWidth));

                bool isFrameOutsideOfRange = !(selectedMessageLo >= lo && selectedMessageHi <= hi);
                if (isFrameOutsideOfRange && KeyboardArrowKeyDirection == Direction.Up)
                    ScrollPosition.y = cells.Take(SelectedMessage).Sum(c => c.GetHeight(buttonWidth));
                else if (isFrameOutsideOfRange && KeyboardArrowKeyDirection == Direction.Down)
                    ScrollPosition.y = cells.Take(SelectedMessage+1).Sum(c => c.GetHeight(buttonWidth)) - WindowRect.height;
            }

            for (int i = firstRenderLogIndex; i <= lastRenderLogIndex; i++)
            {
                var rectButton = new Rect(x: 0, y: buttonY, width: viewWidth, height: cells[i].GetHeight(buttonWidth));
                var isSelected = cells[i].SelectedMessageWhenClicked == SelectedMessage;
                DrawBackground(rectButton, cells[i].BackgroundStyle, isSelected);
                if (IsRepaintEvent)
                    cells[i].MessageStyle.Draw(rectButton, cells[i].Message, false, false, isSelected, false);

                bool messageClicked = IsClicked(rectButton);
                if (messageClicked)
                {
                    bool isLeftClick = Event.current.button == 0;
                    if (!isSelected)
                        LastTimeClicked = 0.0f;
                    if (isLeftClick && !IsDoubleClick)
                        SelectedMessage = cells[i].SelectedMessageWhenClicked;
                    if (!isLeftClick)
                        DrawPopup(cells[i].Message);
                    if (isLeftClick && SelectedMessage == cells[i].SelectedMessageWhenClicked)
                    {
                        if (IsDoubleClick)
                        {
                            LastTimeClicked = 0.0f;
                            BluUtils.JumpToSourceFile(_selectedLog, cells[i].StackTraceIndex);
                        }
                        else
						{
                            LastTimeClicked = EditorApplication.timeSinceStartup;
						}
                    }
                }

                buttonY += cells[i].GetHeight(buttonWidth);
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

        private int GetFirstGreaterCellIndex(List<BluDetailWindowCell> cells, float position, float offset, float buttonWidth)
        {
            float sum = 0f;
            for (int i = 0; i < cells.Count; i++)
            {
                if (position + offset <= sum)
                    return i;

                sum += cells[i].GetHeight(buttonWidth);
            }

            return cells.Count - 1;
        }

        private List<BluDetailWindowCell> GetCells(BluLog log)
        {
            var cells = new List<BluDetailWindowCell>();

            cells.Add(new BluDetailWindowCell(
                message: log.Message,
                selectedMessageWhenClicked: 0,
                stackTraceIndex: 0,
                messageStyle: BluConsoleSkin.MessageDetailCallstackStyle,
                backgroundStyle: BluConsoleSkin.GetLogBackStyle(1)
            ));

            for (int i = 0; i < log.StackTrace.Count; i++)
            {
                cells.Add(new BluDetailWindowCell(
                    message: log.StackTrace[i].FrameInformation,
                    selectedMessageWhenClicked: i+1,
                    stackTraceIndex: i,
                    messageStyle: BluConsoleSkin.MessageDetailCallstackStyle,
                    backgroundStyle: BluConsoleSkin.GetLogBackStyle(1)
                ));
            }

            return cells;
        }

	}

}
