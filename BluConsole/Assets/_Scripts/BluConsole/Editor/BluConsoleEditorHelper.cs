#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;


namespace BluConsole.Editor
{

public static class BluConsoleEditorHelper
{

    #region Buttons

    public static bool ButtonClamped(string text,
                                     GUIStyle style)
    {
        return GUILayout.Button(text, style, GUILayout.MaxWidth(style.CalcSize(new GUIContent(text)).x));
    }

    public static bool ToggleClamped(bool state,
                                     string text,
                                     GUIStyle style)
    {
        return GUILayout.Toggle(state, text, style, GUILayout.MaxWidth(style.CalcSize(new GUIContent(text)).x));
    }

    public static bool ToggleClamped(bool state,
                                     GUIContent content,
                                     GUIStyle style)
    {
        return GUILayout.Toggle(state, content, style, GUILayout.MaxWidth(style.CalcSize(content).x));
    }

    #endregion


    #region GUIContent

    public static GUIContent InfoGUIContent(string text)
    {
        return new GUIContent(text, InfoIcon);
    }

    public static GUIContent InfoGUIContent(int value)
    {
        return new GUIContent(value.ToString(), InfoIcon);
    }

    public static GUIContent WarningGUIContent(string text)
    {
        return new GUIContent(text, WarningIcon);
    }

    public static GUIContent WarningGUIContent(int value)
    {
        return new GUIContent(value.ToString(), WarningIcon);
    }

    public static GUIContent ErrorGUIContent(string text)
    {
        return new GUIContent(text, ErrorIcon);
    }

    public static GUIContent ErrorGUIContent(int value)
    {
        return new GUIContent(value.ToString(), ErrorIcon);
    }

    #endregion


    #region Textures

    public static Texture2D InfoIcon
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
            return EditorGUIUtility.FindTexture("d_console.warnicon.sml");
        }
    }

    public static Texture2D ErrorIcon
    {
        get
        {
            return EditorGUIUtility.FindTexture("d_console.erroricon.sml");
        }
    }

    public static Texture2D BlueTexture
    {
        get
        {
            foreach (var style in GUI.skin.customStyles) {
                if (style.name == "LODSliderRangeSelected") {
                    return style.normal.background;
                }
            }
            return EditorGUIUtility.whiteTexture;
        }
    }

    public static Texture2D GetTexture(Color color)
    {
        Color[] pix = new Color[1];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = color;
        Texture2D result = new Texture2D(1, 1);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    #endregion Textures


    #region Styles

    public static GUIStyle ToolbarSearchTextField
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

    #endregion


    #region ETC

    public static Color ColorFromRGB(int r,
                                     int g,
                                     int b)
    {
        return new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f);
    }

    #endregion
	
}

}
#endif