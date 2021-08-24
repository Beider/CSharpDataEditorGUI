using Godot;
using System;

public static class Constants
    {
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
	public const int KEY_DELETE = (int)KeyList.Shift;
}