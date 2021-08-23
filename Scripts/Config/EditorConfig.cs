using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using CSharpDataEditorDll;

public partial class ConfigSettingsJson
{
    [JsonProperty("projects")]
    public List<ConfigProjects> Projects = new List<ConfigProjects>();
}

[CSDODisplayNameOverride(nameof(Name))]
[CSDOVisibilityModifierStatic(null, "", typeof(ConfigProjects), nameof(ChildrenVisible))]
public partial class ConfigProjects
{
    [JsonProperty("name")]
    [CSDOColorRenderer(nameof(Colors.Green))]
    [CSDOVisibilityModifier]
    public string Name = DEFAULT_NAME;

    [JsonProperty("binarylocation")]
    [CSDOColorRenderer(nameof(Colors.Red))]
    public string BinaryLocation = "";

    [JsonProperty("scanintervalms")]
    public int CommandScanIntervalMs = 250;

    [JsonProperty("commandsfolder")]
    public string CommandFileFolder = "";

    [JsonProperty("editors")]
    public List<ConfigEditors> Editors = new List<ConfigEditors>();
}

[CSDODisplayNameOverride(nameof(Name))]
[CSDOVisibilityModifierStatic(null, "", typeof(ConfigEditors), nameof(ChildrenVisible))]
public partial class ConfigEditors
{
    [JsonProperty("name")]
    [CSDOVisibilityModifier]
    public string Name = DEFAULT_NAME;

    // Used for sending commands
    [JsonProperty("commandkey")]
    [CSDOListRendererStatic(typeof(ConfigEditors), nameof(GetStaticList))]
    public string CommandKey = "";

    [JsonProperty("datareader")]
    [CSDOListRendererStatic(typeof(ConfigEditors), nameof(GetStaticList))]
    public string DataReader = "";

    [JsonProperty("datareaderparam")]
    [CSDOVisibilityModifierStatic(typeof(ConfigEditors), nameof(IsDataReaderParamVisible))]
    public string DataReaderParam = "";

    [JsonProperty("dataType")]
    [CSDOListRendererEnum(typeof(DataTypes), nameof(Colors.LightBlue))]
    public string DataType = "";

    [JsonProperty("autosave")]
    public bool AutoSave = false;

    [JsonProperty("autofront")]
    public bool AutoBringToFront = false;

    public enum DataTypes
    {
        TEST, TEST2, HELLO
    }

    public static string[] GetStaticList(CSDataObject dataObject)
    {
        List<string> myList = new List<string>();
        myList.Add("- Select Data Reader -");
        myList.Add("A value2");
        myList.Add("A second value");
        return myList.ToArray();
    }

    public static bool IsDataReaderParamVisible(CSDataObject dataObject)
    {
        CSDataObjectMember member = (CSDataObjectMember) ((CSDataObjectClass) dataObject.Parent).FindMemberByName(nameof(DataReader));
        return member.CurrentValue != "- Select Data Reader -";
    }
}