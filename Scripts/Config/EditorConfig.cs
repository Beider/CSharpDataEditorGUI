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
[CSDOStartCollapsed]
public partial class ConfigProjects
{
    [JsonProperty("name")]
    [CSDOVisibilityModifier]
    public string Name = DEFAULT_NAME;

    [JsonProperty("binarylocation")]
    public string BinaryLocation = "";

    [JsonProperty("scanintervalms")]
    [CSDOVisibilityModifierMember(nameof(BinaryLocation), "a")]
    public int CommandScanIntervalMs = 250;

    [JsonProperty("commandsfolder")]
    [CSDOVisibilityModifierMember(nameof(BinaryLocation), "a")]
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
    public string CommandKey = "";

    [JsonProperty("datareader")]
    public string DataReader = "";

    [JsonProperty("datareaderparam")]
    public string DataReaderParam = "";

    [JsonProperty("dataType")]
    public string DataType = "";

    [JsonProperty("autosave")]
    public bool AutoSave = false;

    [JsonProperty("autofront")]
    public bool AutoBringToFront = false;
}