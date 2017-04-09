using UnityEditor;
using UnityEngine;


namespace BluConsole.Editor
{

public class BluConsoleSkin
{

    #region Texture

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


    #endregion Texture


    #region Style

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
            return GUI.skin.FindStyle("CN EntryInfo");
        }
    }

    public static GUIStyle LogWarnStyle
    {
        get
        {
            return GUI.skin.FindStyle("CN EntryWarn");
        }
    }

    public static GUIStyle LogErrorStyle
    {
        get
        {
            return GUI.skin.FindStyle("CN EntryError");
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
