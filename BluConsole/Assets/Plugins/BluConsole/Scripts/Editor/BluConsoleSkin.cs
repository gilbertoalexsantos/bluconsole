using UnityEditor;
using UnityEngine;
using BluConsole.Core;


namespace BluConsole.Editor
{

    public static class BluConsoleSkin
    {

        #region Texture

        public static Texture ConsoleIcon
        {
            get
            {
                return BluLogFiles.Instance.WindowIcon;
            }
        }

        public static Texture2D EvenBackTexture
        {
            get
            {
                return EvenBackStyle.normal.background;
            }
        }

        public static Texture2D OddBackTexture
        {
            get
            {
                return OddBackStyle.normal.background;
            }
        }

        public static Texture2D InfoIcon
        {
            get
            {
                return EditorGUIUtility.FindTexture("d_console.infoicon");
            }
        }

        public static Texture2D InfoIconSmall
        {
            get
            {
                return EditorGUIUtility.FindTexture("d_console.infoicon.sml");
            }
        }

        public static Texture2D WarningIcon
        {
            get
            {
                return EditorGUIUtility.FindTexture("d_console.warnicon");
            }
        }

        public static Texture2D WarningIconSmall
        {
            get
            {
                return EditorGUIUtility.FindTexture("d_console.warnicon.sml");
            }
        }

        public static Texture2D ErrorIcon
        {
            get
            {
                return EditorGUIUtility.FindTexture("d_console.erroricon");
            }
        }

        public static Texture2D ErrorIconSmall
        {
            get
            {
                return EditorGUIUtility.FindTexture("d_console.erroricon.sml");
            }
        }

        public static GUIStyle GetLogListStyle(BluLogType logType)
        {
            switch (logType)
            {
                case BluLogType.Normal:
                    return LogInfoStyle;
                case BluLogType.Warning:
                    return LogWarnStyle;
                case BluLogType.Error:
                    return LogErrorStyle;
            }
            return LogInfoStyle;
        }

        #endregion Texture


        #region Style

        public static GUIStyle MessageDetailCallstackStyle
        {
            get
            {
                return new GUIStyle(BluConsoleSkin.MessageStyle)
                {
                    stretchWidth = true,
                    wordWrap = true,
                    onNormal = {textColor = GetLogListStyle(BluLogType.Normal).onNormal.textColor}
                };
            }
        }

        public static GUIStyle GetLogBackStyle(int row)
        {
            return row % 2 == 0 ? BluConsoleSkin.EvenBackStyle : BluConsoleSkin.OddBackStyle;
        }

        public static GUIStyle BoxStyle
        {
            get
            {
                return GUI.skin.FindStyle("CN Box");
            }
        }

        public static GUIStyle CollapseStyle
        {
            get
            {
                return GUI.skin.FindStyle("CN CountBadge");
            }
        }

        public static GUIStyle ButtonStyle
        {
            get
            {
                return GUI.skin.FindStyle("Button");
            }
        }

        public static GUIStyle ToolbarButtonStyle
        {
            get
            {
                return GUI.skin.FindStyle("ToolbarButton");
            }
        }

        public static GUIStyle ToolbarSearchTextFieldStyle
        {
            get
            {
                return GUI.skin.FindStyle("ToolbarSeachTextField");
            }
        }

        public static GUIStyle ToolbarSearchCancelButtonStyle
        {
            get
            {
                return GUI.skin.FindStyle("ToolbarSeachCancelButton");
            }
        }

        public static GUIStyle EvenBackStyle
        {
            get
            {
                return GUI.skin.FindStyle("CN EntryBackEven");
            }
        }

        public static GUIStyle OddBackStyle
        {
            get
            {
                return GUI.skin.FindStyle("CN EntryBackOdd");
            }
        }

        public static GUIStyle MessageStyle
        {
            get
            {
                return GUI.skin.FindStyle("CN Message");
            }
        }

        public static GUIStyle LogInfoStyle
        {
            get
            {
                var style = new GUIStyle(GUI.skin.FindStyle("CN EntryInfo"));
                style.alignment = TextAnchor.MiddleLeft;
                style.contentOffset = BluLogConfiguration.Instance.ListLogContentOffset;
                style.fontSize = BluLogConfiguration.Instance.ListLogSize;
#if UNITY_2017_3_OR_NEWER
                style.normal.background = InfoIcon;
#endif
                return style;
            }
        }

        public static GUIStyle LogWarnStyle
        {
            get
            {
                var style = new GUIStyle(GUI.skin.FindStyle("CN EntryWarn"));
                style.alignment = TextAnchor.MiddleLeft;
                style.contentOffset = BluLogConfiguration.Instance.ListLogContentOffset;
                style.fontSize = BluLogConfiguration.Instance.ListLogSize;
#if UNITY_2017_3_OR_NEWER
                style.normal.background = WarningIcon;
#endif
                return style;
            }
        }

        public static GUIStyle LogErrorStyle
        {
            get
            {
                var style = new GUIStyle(GUI.skin.FindStyle("CN EntryError"));
                style.alignment = TextAnchor.MiddleLeft;  
                style.contentOffset = BluLogConfiguration.Instance.ListLogContentOffset;
                style.fontSize = BluLogConfiguration.Instance.ListLogSize;
#if UNITY_2017_3_OR_NEWER
                style.normal.background = ErrorIcon;
#endif
                return style;
            }
        }

        public static GUIStyle ToolbarStyle
        {
            get
            {
                return GUI.skin.FindStyle("Toolbar");
            }
        }

        #endregion Style

    }

}

