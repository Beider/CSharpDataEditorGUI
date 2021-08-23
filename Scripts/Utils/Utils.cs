using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;

public static class Utils
{
    private static Dictionary<string, Texture> ImageLookup = new Dictionary<string, Texture>();

    private static Dictionary<string, Color> ColorLookup = new Dictionary<string, Color>();

    public static Texture LoadTextureFromFile(string path)
    {
        if (!ImageLookup.ContainsKey(path))
        {
            StreamTexture streamTexture = (StreamTexture)ResourceLoader.Load(path);
            ImageLookup.Add(path, streamTexture);
        }
        return ImageLookup[path];
    }

    /// <summary>
    /// Resolves the color to use, uses Colors.Transparent as no color
    /// </summary>
    /// <param name="colorName">The name of the color to fetch</param>
    /// <returns>The color to use or transparent if none should be used</returns>
    public static Color ResolveColorFromString(string colorName)
    {
        if (colorName == null)
        {
            return Colors.Transparent;
        }

        if (ColorLookup.Count == 0)
        {
            LoadColors();
        }

        Color color = Colors.Transparent;
        string colorNameLower = colorName.ToLower();
        if (ColorLookup.ContainsKey(colorNameLower))
        {
            return ColorLookup[colorNameLower];
        }

        return color;
    }

    private static void LoadColors()
        {
            Type colorType = typeof(Colors);
            PropertyInfo[] props = colorType.GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach (PropertyInfo info in props)
            {
                ColorLookup.Add(info.Name.ToLower(), (Color)info.GetValue(null));
            }
        }
}
