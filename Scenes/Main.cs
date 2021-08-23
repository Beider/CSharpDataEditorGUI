using Godot;
using System;
using System.Reflection;
using CSharpDataEditorDll;

public class Main : Control
{
	private CSDataObjectTree DisplayTree;

	private CSDataObjectClass DataObjectClass;

	private NewtonsoftJsonConverter Converter;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		DisplayTree = FindNode("CSDataObjectTree") as CSDataObjectTree;
		Reload();
	}

	private void Reload()
	{
		GD.Print("Reloading");
		Converter = new NewtonsoftJsonConverter();
		Converter.Init("E:\\Coding\\Godot\\CSharpDataEditor\\TestData", nameof(ConfigSettingsJson), Assembly.GetExecutingAssembly().Location);
		DataObjectClass = Converter.GetObject("test");
		DisplayTree.InitTree(DataObjectClass);
	}

	private void OnSavePressed()
	{
		Converter.SaveObject("test", DataObjectClass);
		Reload();
	}

}
