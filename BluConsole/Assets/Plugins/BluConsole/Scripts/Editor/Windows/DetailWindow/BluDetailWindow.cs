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
        private float[] sums;

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

            float buttonWidth = DefaultButtonWidth;

            var cells = GetCells(_selectedLog);
            foreach (var cell in cells)
                cell.Message = GetTruncatedDetailMessage(cell.Message);
            sums = new float[cells.Count+1];
            sums[0] = 0;
            for (int i = 1; i <= cells.Count; i++)
                sums[i] = cells[i-1].GetHeight(buttonWidth) + sums[i-1];

            float viewHeight = sums[cells.Count];
            if (viewHeight > WindowRect.height)
                buttonWidth -= 15f;

            for (int i = 1; i <= cells.Count; i++)
                sums[i] = cells[i-1].GetHeight(buttonWidth) + sums[i-1];
            viewHeight = sums[cells.Count];

            float viewWidth = buttonWidth;

            Rect scrollViewPosition = WindowRect;
            Rect scrollViewViewRect = new Rect(x: 0f, y: 0f, width: viewWidth, height: viewHeight);

            ScrollPosition = GUI.BeginScrollView(position: scrollViewPosition,
                                                 scrollPosition: ScrollPosition,
                                                 viewRect: scrollViewViewRect);

            int firstRenderLogIndex = GetFirstGreaterCellIndex(cells, ScrollPosition.y, 0f) - 1;
            firstRenderLogIndex = Mathf.Clamp(firstRenderLogIndex, 0, cells.Count - 1);
            int lastRenderLogIndex = GetFirstGreaterCellIndex(cells, ScrollPosition.y, WindowRect.height);
            lastRenderLogIndex = Mathf.Clamp(lastRenderLogIndex, 0, cells.Count - 1);

            float buttonY = sums[firstRenderLogIndex];
            
            // Handling up/down arrow keys
            if (HasKeyboardArrowKeyInput)
            {
                float lo = ScrollPosition.y;
                float hi = ScrollPosition.y + WindowRect.height;
                float selectedMessageLo = sums[SelectedMessage];
                float selectedMessageHi = sums[SelectedMessage+1];

                bool isFrameOutsideOfRange = !(selectedMessageLo >= lo && selectedMessageHi <= hi);
                if (isFrameOutsideOfRange && KeyboardArrowKeyDirection == Direction.Up)
                    ScrollPosition.y = selectedMessageLo;
                else if (isFrameOutsideOfRange && KeyboardArrowKeyDirection == Direction.Down)
                    ScrollPosition.y = selectedMessageHi - WindowRect.height;
            }

            for (int i = firstRenderLogIndex; i <= lastRenderLogIndex; i++)
            {
                float cellHeight = sums[i+1] - sums[i];
                var rectButton = new Rect(x: 0, y: buttonY, width: viewWidth, height: cellHeight);
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

                buttonY += cellHeight;
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

        private int GetFirstGreaterCellIndex(List<BluDetailWindowCell> cells, float position, float offset)
        {
            float sum = 0f;
            for (int i = 0; i < cells.Count; i++)
            {
                if (position + offset <= sum)
                    return i;

                sum += sums[i+1] - sums[i];
            }

            return cells.Count - 1;
        }

        private List<BluDetailWindowCell> GetCells(BluLog log)
        {
            var cells = new List<BluDetailWindowCell>(1 + log.StackTrace.Count);

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
