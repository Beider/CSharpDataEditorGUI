using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using CSharpDataEditorDll;

public class Settings : Node
{
    public const string SETTINGS_FILE_NAME = "editor_settings";

    public delegate void EventOnSettingsReFreshed();
    public event EventOnSettingsReFreshed OnSettingsRefresh = delegate { };

    public static Settings Instance;

    public static bool CollapseOnDrag {get; private set;} = true;

    public ConfigSettingsJson Configuration {get; private set;} = new ConfigSettingsJson();

    private Dictionary<string, PackedScene> RendererTypes = new Dictionary<string, PackedScene>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
        ReloadConfiguration();
        InitRenderers();
    }

    private void InitRenderers()
    {
        PackedScene rendererScene = ResourceLoader.Load("res://Scenes/Renderers/ListRenderer.tscn") as PackedScene;
        RendererTypes.Add(CSDOList.LIST_RENDERER_TYPE, rendererScene);
    }

    public static void ReloadConfiguration()
    {
        bool configLoaded = false;
        string path = SettingsLocation();
        try
        {
            if (!path.EndsWith("/") && ! path.EndsWith("\\"))
            {
                path += "/";
            }
            path += SETTINGS_FILE_NAME + ".json";
            if (System.IO.File.Exists(path))
            {
                Instance.Configuration = Utils.ReadJsonFile<ConfigSettingsJson>(path);
                configLoaded = true;
            }
        } 
        catch (Exception ex)
        {
            GD.PrintErr(ex.StackTrace);
        }
        if (!configLoaded)
        {
            Instance.Configuration = new ConfigSettingsJson();
            string json = Utils.ToJson(Instance.Configuration);
            try
            {
                System.IO.File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                GD.Print(ex.Message);
            }
        }
        Instance.OnSettingsRefresh();
    }

    /// <summary>
    /// Get the scene for the renderer type
    /// </summary>
    /// <param name="rendererType">The type of renderer</param>
    /// <returns>The renderer scene or null if not found</returns>
    public static PackedScene GetRendererScene(string rendererType)
    {
        if (Instance.RendererTypes.ContainsKey(rendererType))
        {
            return Instance.RendererTypes[rendererType];
        }
        return null;
    }

    public static string SettingsLocation()
    {
        return Utils.GetBinaryFolder();
    }

}
