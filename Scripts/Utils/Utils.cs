using Godot;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

    public static T ReadJsonFile<T>(string fullPath)
    {
        object createdObject = null;
        if (System.IO.File.Exists(fullPath))
        {
            string json = System.IO.File.ReadAllText(fullPath);
            createdObject = FromJson(json, typeof(T));
        }

        if (createdObject == null)
        {
            createdObject = Activator.CreateInstance(typeof(T));
        }
        
        return (T)createdObject;
    }

    public static object FromJson(string json, Type type) => JsonConvert.DeserializeObject(json, type, Settings);

    public static string ToJson(object jsonObject) => JsonConvert.SerializeObject(jsonObject);

    /// <summary>
    /// Settings for JSON serializer
    /// </summary>
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        Formatting = Formatting.Indented,
        DateParseHandling = DateParseHandling.None,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore,
        Converters = {
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
        },
    };

    public static string GetBinaryFolder()
    {
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    public static string GetBinaryString()
    {
        return Assembly.GetExecutingAssembly().FullName;
    }

    public static List<Type> GetTypesSafe(this Assembly assembly)
    {
        List<Type> typeList = new List<Type>();
        try
        {
            typeList = new List<Type>(assembly.GetTypes());
        }
        catch (ReflectionTypeLoadException ex)
        {
            foreach (Type type in ex.Types)
            {
                if (type != null)
                {
                    typeList.Add(type);
                }
            }
        }
        
        return typeList;
    }
}
