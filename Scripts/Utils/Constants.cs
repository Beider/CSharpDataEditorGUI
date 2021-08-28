using Godot;
using System;
using System.Reflection;
using CSharpDataEditorDll;

public static class Constants
{
    public const string COMMAND_FILE_NAME = "commands.csdelst";
    public const string COMMAND_SEPARATOR = ":";
    public const string METADATA_COLLAPSED = "Collapsed";
    public const string METADATA_COLLAPSED_DRAG = "Collapsed_Drag";
    public const string METADATA_DISPLAY_OVERRIDE = "DisplayOverride";
    public const string METADATA_DISPLAY_OVERRIDE_TARGET = "OverrideBy";
    public const string METADATA_VISMOD_SELF = "VMSelf";
    public const string METADATA_VISMOD_CHILDREN = "VMChildren";
    public const string METADATA_TREE_ITEM = "TreeItem";
    public const string METADATA_EDITABLE_COLUMN_NUM = "EditableColumnNum";
    public const string IMAGE_ADD = "res://Assets/Images/add.png";
	public const string IMAGE_REMOVE = "res://Assets/Images/remove.png";
	public const string IMAGE_ERROR = "res://Assets/Images/hazard-sign.png";
	public const string MESSAGE_DELETE_KEY = "(Hold SHIFT when pressing to delete)";
	public const string MESSAGE_ERROR_KEY = "(Hold SHIFT and click to roll back)";
    public const string NO_OBJECTS_FOUND = "No objects found";
	public const int KEY_DELETE = (int)KeyList.Shift;

    public const string COLOR_PROJECT = nameof(Colors.Green);
    public const string COLOR_EDITOR = nameof(Colors.MediumPurple);

    private static ConfigProjects SettingsProject = null;

    public static ConfigProjects GetSettingsProject()
    {
        if (SettingsProject == null)
        {
            SettingsProject = new ConfigProjects();
            SettingsProject.Name = "C# Editor";
            SettingsProject.BinaryLocation = Assembly.GetExecutingAssembly().Location;
            ConfigEditors editor = new ConfigEditors();
            editor.DataType = nameof(ConfigSettingsJson);
            editor.Name = "Settings";
            editor.DataConverter = typeof(NewtonsoftJsonConverter).FullName;
            editor.DataConverterParam = Settings.SettingsLocation();
            editor.AllowCreateNew = false;
            editor.AllowOpen = false;
            SettingsProject.Editors.Add(editor);
        }
        return SettingsProject;
    }
}