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
    public class BluListWindow : BluConsoleWindow
    {

        private bool IsFollowScroll { get; set; }

        public override void OnGUI(int id)
        {
            base.OnGUI(id);

            UnityLoggerServer.StartGettingLogs();

            SelectedMessage = Mathf.Clamp(SelectedMessage, 0, QtLogs - 1);

            float buttonWidth = DefaultButtonWidth;
            if (QtLogs * DefaultButtonHeight > WindowRect.height)
                buttonWidth -= 15f;

            float viewWidth = buttonWidth;
            float viewHeight = QtLogs * DefaultButtonHeight;

            Rect scrollWindowRect = WindowRect;
            Rect scrollViewRect = new Rect(x: 0f, y: 0f, width: viewWidth, height: viewHeight);

            GUI.DrawTexture(scrollWindowRect, BluConsoleSkin.EvenBackTexture);

            Vector2 oldScrollPosition = ScrollPosition;
            ScrollPosition = GUI.BeginScrollView(position: scrollWindowRect,
                                                 scrollPosition: ScrollPosition,
                                                 viewRect: scrollViewRect);

            int firstRenderLogIndex = (int)(ScrollPosition.y / DefaultButtonHeight);
            firstRenderLogIndex = Mathf.Clamp(firstRenderLogIndex, 0, QtLogs);

            int lastRenderLogIndex = firstRenderLogIndex + (int)(WindowRect.height / DefaultButtonHeight) + 2;
            lastRenderLogIndex = Mathf.Clamp(lastRenderLogIndex, 0, QtLogs);

            // Handling up/down arrow keys
            if (HasKeyboardArrowKeyInput)
            {
                bool isFrameOutsideOfRange = SelectedMessage < firstRenderLogIndex + 1 ||
                                             SelectedMessage > lastRenderLogIndex - 3;
                if (isFrameOutsideOfRange && KeyboardArrowKeyDirection == Direction.Up)
                {
                    ScrollPosition.y = DefaultButtonHeight * SelectedMessage;
                }
                else if (isFrameOutsideOfRange && KeyboardArrowKeyDirection == Direction.Down)
                {
                    int md = lastRenderLogIndex - firstRenderLogIndex - 3;
                    float ss = md * DefaultButtonHeight;
                    float sd = WindowRect.height - ss;
                    ScrollPosition.y = (DefaultButtonHeight * (SelectedMessage + 1) - ss - sd);
                }
            }

            float buttonY = firstRenderLogIndex * DefaultButtonHeight;
            bool hasCollapse = UnityLoggerServer.HasFlag(ConsoleWindowFlag.Collapse);
            bool hasSomeClick = false;
            for (int i = firstRenderLogIndex; i < lastRenderLogIndex; i++)
            {
                int row = Rows[i];
                BluLog log = Logs[i];
                var styleBack = BluConsoleSkin.GetLogBackStyle(i);

                var styleMessage = BluConsoleSkin.GetLogListStyle(log.LogType);
                string showMessage = GetTruncatedMessage(log);
                var contentMessage = new GUIContent(showMessage);
                var rectMessage = new Rect(x: 0, y: buttonY, width: viewWidth, height: DefaultButtonHeight);
                bool isSelected = i == SelectedMessage;
                
                DrawBackground(rectMessage, styleBack, isSelected);
                if (IsRepaintEvent)
                    styleMessage.Draw(rectMessage, contentMessage, false, false, isSelected, false);

                bool messageClicked = IsClicked(rectMessage);
                bool isLeftClick = messageClicked && Event.current.button == 0;

                if (hasCollapse)
                {
                    int quantity = UnityLoggerServer.GetLogCount(row);
                    var collapseCount = Mathf.Min(quantity, LogConfiguration.MaxLengthCollapse);
                    var collapseText = collapseCount.ToString();
                    if (collapseCount >= LogConfiguration.MaxLengthCollapse)
                        collapseText += "+";
                    var collapseContent = new GUIContent(collapseText);
                    var collapseSize = BluConsoleSkin.CollapseStyle.CalcSize(collapseContent);

                    var collapseRect = new Rect(x: viewWidth - collapseSize.x - 5f,
                                                y: (buttonY + buttonY + DefaultButtonHeight - collapseSize.y) * 0.5f,
                                                width: collapseSize.x,
                                                height: collapseSize.y);

                    GUI.Label(collapseRect, collapseContent, BluConsoleSkin.CollapseStyle);
                }

                if (messageClicked)
                {
                    hasSomeClick = true;
                    if (SelectedMessage != i)
                        LastTimeClicked = 0.0f;
                    if (isLeftClick)
                    {
                        BluUtils.PingLog(GetCompleteLog(row));
                        SelectedMessage = i;
                    }
                    if (!isLeftClick)
                        DrawPopup(log);
                    if (isLeftClick && i == SelectedMessage)
                    {
                        if (IsDoubleClick)
                        {
                            LastTimeClicked = 0.0f;
                            var completeLog = GetCompleteLog(row);
                            BluUtils.JumpToSourceFile(completeLog, 0);
                        }
                        else
                            LastTimeClicked = EditorApplication.timeSinceStartup;
                    }
                }

                buttonY += DefaultButtonHeight;
            }

            GUI.EndScrollView();

            if (IsScrollUp || hasSomeClick)
            {
                IsFollowScroll = false;
            }
            else if (ScrollPosition != oldScrollPosition)
            {
                IsFollowScroll = false;
                float topOffset = viewHeight - WindowRect.height;
                if (ScrollPosition.y >= topOffset)
                    IsFollowScroll = true;
            }

            if (IsFollowScroll)
                ScrollPosition.y = viewHeight - WindowRect.height;

            UnityLoggerServer.StopGettingsLogs();
        }

        protected override void OnEnterKeyPressed()
        {
            BluLog log = null;
            if (SelectedMessage != -1 &&
                SelectedMessage >= 0 &&
                SelectedMessage < QtLogs)
            {
                log = GetCompleteLog(Rows[SelectedMessage]);
            }

            if (log != null)
                BluUtils.JumpToSourceFile(log, 0);
        }
        
    }

}
