using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using CSharpDataEditorDll;

public partial class ConfigSettingsJson
{
    [JsonProperty("autocollapse")]
    [CSDODescription("If set to true tree items will automatically be collapsed when dragging to reorder")]
    public bool AutoCollapse = true;

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
    [CSDODescription("The project name, used for the UI")]
    public string Name = DEFAULT_NAME;

    [JsonProperty("binarylocation")]
    [CSDOColorRenderer(COLOR_BINARY)]
    [CSDODescription("The binary this project works against")]
    public string BinaryLocation = "";

    [JsonProperty("commandsfolder")]
    [CSDODescription("If you want to use external command files for this project then set the path here")]
    public string CommandFileFolder = "";

    [JsonProperty("scanintervalms")]
    [CSDODescription("How often should we check for command updates in milliseconds")]
    [CSDOVisibilityModifierStatic(typeof(ConfigProjects), nameof(IsCommandIntervalMsVisible))]
    public int CommandScanIntervalMs = 250;

    [JsonProperty("editors")]
    public List<ConfigEditors> Editors = new List<ConfigEditors>();
}

[CSDODisplayNameOverride(nameof(Name))]
[CSDOVisibilityModifierStatic(null, "", typeof(ConfigEditors), nameof(ChildrenVisible))]
public partial class ConfigEditors
{
    [JsonProperty("name")]
    [CSDOColorRenderer(nameof(Colors.BlueViolet))]
    [CSDOVisibilityModifier]
    [CSDODescription("The name of this editor, used for UI")]
    public string Name = DEFAULT_NAME;

    // Used for sending commands
    [JsonProperty("commandkey")]
    [CSDODescription("The command key for this editor, used for external commands through command files")]
    public string CommandKey = "";

    [JsonProperty("dataconverter")]
    [CSDOListRendererStatic(typeof(ConfigEditors), nameof(GetDataConverterList), nameof(GetDataConverterColor))]
    [CSDODescription("The data converter used for this editor")]
    public string DataConverter = DATA_CONVERTER_DEFAULT_NAME;

    [JsonProperty("datareaderparam")]
    [CSDOVisibilityModifierStatic(typeof(ConfigEditors), nameof(IsDataReaderParamVisible))]
    [CSDODescription("The parameter for this editor, most likely the folder where to find the files. Depends on converter.")]
    public string DataConverterParam = "";

    [JsonProperty("dataType")]
    [CSDOListRendererStatic(typeof(ConfigEditors), nameof(GetDataTypeList), nameof(GetDataTypeColor))]
    [CSDODescription("The type that the data converter should convert the data into")]
    public string DataType = "";

    [JsonProperty("autosave")]
    [CSDODescription("If set to true changes will be automatically saved when you switch file manually or an external command is recieved")]
    public bool AutoSave = false;

    [JsonProperty("autofront")]
    [CSDODescription("If set to true this will be brought to front when an external command is recieved")]
    public bool AutoBringToFront = false;
}