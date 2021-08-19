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

// TODO: Add color and such
[CSDODisplayNameOverride(nameof(Name))]
public partial class ConfigProjects
{
    [JsonProperty("name")]
    public string Name = "";

    [JsonProperty("binarylocation")]
    public string BinaryLocation = "";

    [JsonProperty("scanintervalms")]
    public int CommandScanIntervalMs = 250;

    [JsonProperty("commandsfolder")]
    public string CommandFileFolder = "";

    [JsonProperty("editors")]
    public List<ConfigEditors> Editors = new List<ConfigEditors>();
}

public partial class ConfigEditors
{
    [JsonProperty("name")]
    public string Name = "";

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