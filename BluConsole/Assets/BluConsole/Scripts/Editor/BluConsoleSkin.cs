﻿/*
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
using System.Runtime.InteropServices;


namespace BluConsole.Editor
{

public class BluConsoleSkin
{

	#region Color


	public static Color SizerLineCenterColour
	{
		get
		{
			return BluConsoleEditorHelper.ColorPercent(Color.white, 0.5f);
		}
	}

	public static Color SizerLineBorderColour
	{
		get
		{
			return BluConsoleEditorHelper.ColorPercent(Color.white, 0.55f);
		}
	}

	public static Color EvenErrorBackColor
	{
		get
		{
			return BluConsoleEditorHelper.ColorFromRGB(230, 173, 165);
		}
	}

	public static Color OddErrorBackColor
	{
		get
		{
			return BluConsoleEditorHelper.ColorFromRGB(229, 180, 174);
		}
	}

	public static Color SelectedBackColor
	{
		get
		{
			return new Color(0.5f, 0.5f, 1);
		}
	}


	#endregion


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
			var style = new GUIStyle(GUI.skin.FindStyle("CN Message"));
			style.alignment = TextAnchor.MiddleLeft;
			return style;
		}
	}

	public static GUIStyle LogImageErrorStyle
	{
		get
		{
			return GUI.skin.FindStyle("CN EntryError");
		}
	}

	public static GUIStyle LogImageInfoStyle
	{
		get
		{
			return GUI.skin.FindStyle("CN EntryInfo");
		}
	}

	public static GUIStyle LogImageWarnStyle
	{
		get
		{
			return GUI.skin.FindStyle("CN EntryWarn");
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