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


using UnityEngine;


namespace BluConsole.Editor
{

public class BluConsoleEditorHelper
{

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

}

}
