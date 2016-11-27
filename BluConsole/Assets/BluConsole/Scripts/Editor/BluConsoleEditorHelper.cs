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
using System.Reflection;
using UnityEditor;
using UnityEngine;
using BluConsole.Core;


namespace BluConsole.Editor
{

public static class BluConsoleEditorHelper
{

	// Cached variables
	private static Texture2D _consoleIcon = null;
	private static bool _hasConsoleIcon = true;


	#region Buttons


	public static bool ButtonClamped(
		string text,
		GUIStyle style)
	{
		return GUILayout.Button(text, style, GUILayout.MaxWidth(style.CalcSize(new GUIContent(text)).x));
	}

	public static bool ToggleClamped(
		bool state,
		string text,
		GUIStyle style)
	{
		return GUILayout.Toggle(state, text, style, GUILayout.MaxWidth(style.CalcSize(new GUIContent(text)).x));
	}

	public static bool ToggleClamped(
		bool state,
		GUIContent content,
		GUIStyle style)
	{
		return GUILayout.Toggle(state, content, style, GUILayout.MaxWidth(style.CalcSize(content).x));
	}


	#endregion


	#region GUIContent


	public static GUIContent InfoGUIContent(
		string text)
	{
		return new GUIContent(text, InfoIconSmall);
	}

	public static GUIContent InfoGUIContent(
		int value)
	{
		return new GUIContent(value.ToString(), InfoIconSmall);
	}

	public static GUIContent WarningGUIContent(
		string text)
	{
		return new GUIContent(text, WarningIconSmall);
	}

	public static GUIContent WarningGUIContent(
		int value)
	{
		return new GUIContent(value.ToString(), WarningIconSmall);
	}

	public static GUIContent ErrorGUIContent(
		string text)
	{
		return new GUIContent(text, ErrorIconSmall);
	}

	public static GUIContent ErrorGUIContent(
		int value)
	{
		return new GUIContent(value.ToString(), ErrorIconSmall);
	}


	#endregion


	#region Textures


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

	public static Texture2D BlueTexture
	{
		get
		{
			foreach (var style in GUI.skin.customStyles)
			{
				if (style.name == "LODSliderRangeSelected")
				{
					return style.normal.background;
				}
			}
			return EditorGUIUtility.whiteTexture;
		}
	}

	public static Texture2D ConsoleIcon
	{
		get
		{
            if (_consoleIcon == null && _hasConsoleIcon)
            {
                string path  ="Assets/BluConsole/Images/bluconsole-icon.png";
                _consoleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (_consoleIcon == null)
                    _hasConsoleIcon = false;
            }

			return _consoleIcon;
		}
	}

	public static Texture2D GetTexture(
		Color color)
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


	public static Color ColorFromRGB(
		int r,
		int g,
		int b)
	{
		return new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f);
	}

    public static Color ColorPercent(
        Color color,
        float percent)
    {
        return new Color(color.r * percent, color.g * percent, color.b * percent);
    }


	#endregion

}

}
