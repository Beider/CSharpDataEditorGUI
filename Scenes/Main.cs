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
		Converter = new NewtonsoftJsonConverter();
		Converter.Init("E:\\Coding\\Godot\\CSharpDataEditor\\TestData", typeof(ConfigSettingsJson), Assembly.GetExecutingAssembly());
		DataObjectClass = Converter.GetObject("test");
		//FillDummyConfig(DataObjectClass);
		DisplayTree.InitTree(DataObjectClass);
	}

	private void OnSavePressed()
	{
		Converter.SaveObject("test", DataObjectClass);
		Reload();
	}

	private void FillDummyConfig(CSDataObjectClass dataObjectClass)
	{
		CSDataObjectMemberArray projects = (CSDataObjectMemberArray)dataObjectClass.ClassMembers[0];
		CSDataObjectClass confProjects =  (CSDataObjectClass)projects.AddNew();
		((CSDataObjectMember)confProjects.ClassMembers[0]).SetValue("Some name");

		confProjects =  (CSDataObjectClass)projects.AddNew();
		((CSDataObjectMember)confProjects.ClassMembers[0]).SetValue("OtherName");

		confProjects =  (CSDataObjectClass)projects.AddNew();
		((CSDataObjectMember)confProjects.ClassMembers[0]).SetValue("NoName");
	}

}
