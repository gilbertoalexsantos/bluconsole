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
