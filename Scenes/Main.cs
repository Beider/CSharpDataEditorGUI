using Godot;
using System;
using System.Reflection;
using CSharpDataEditorDll;

public class Main : Control
{
	private CSDataObjectTree Settings;

	private NewtonsoftJsonConverter Converter;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Settings = FindNode("CSDataObjectTree") as CSDataObjectTree;
		Button btnShow = FindNode("BtnShow") as Button;
		btnShow.Connect("pressed", UIManager.Instance, nameof(UIManager.ToggleSideBarVisible));
		UIManager.Instance.CollapsedMenu = FindNode("CollapsedMenu") as Control;
		UIManager.Instance.CollapsedMenu.Visible = false;
		UIManager.Instance.SplitContainer = FindNode("SplitContainer") as HSplitContainer;
		UIManager.Instance.DataContainer = FindNode("DataContainer") as Control;
		UIManager.Instance.SetSaveButtons((Button)FindNode("BtnSaveAll"),(Button)FindNode("BtnSaveCol"));

		// Because renaming the project breaks everything we do this
		OS.SetWindowTitle("C# Data Editor");
	}

	private void OnSavePressed()
	{
		UIManager.Instance.SaveAll();
	}

	private void OnShowSettingsPressed()
	{
		UIManager.Instance.ShowSettings();
	}

}
