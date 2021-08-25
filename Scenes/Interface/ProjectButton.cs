using Godot;
using System;
using System.Reflection;
using CSharpDataEditorDll;

public class ProjectButton : Control
{
	private Button PrjButton = null;
	private Label NameLabel = null;
	private ConfigProjects Project = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PrjButton = FindNode("Button") as Button;
		PrjButton.Connect("pressed", this, nameof(OnButtonPressed));
		NameLabel = FindNode("Name") as Label;
		UpdateUI();
	}

	public void InitButton(ConfigProjects project)
	{
		Project = project;
        UpdateUI();

        // TODO: Some debug code to remove
		GD.Print($"Loading project {project.Name}");
		if (project.IsValid())
		{
			if (project.Editors != null)
			{
				foreach (ConfigEditors editor in project.Editors)
				{
					if (editor.DataConverter != null && editor.DataConverter != "" 
						&& editor.DataConverterParam != null && editor.DataConverterParam != "")
					{
						IDataConverter converter = new NewtonsoftJsonConverter();
						converter.Init(editor.DataConverterParam, editor.DataType, project.BinaryLocation);
						foreach (string objectName in converter.GetValidObjectNames())
						{
							GD.Print($"{editor.Name} object: {objectName}");
						}
					}
				}
			}
		}
	}

	private void OnButtonPressed()
	{
		if (Project != null)
		{
			GD.Print($"Loading project {Project.Name}");
		}
	}

	private void UpdateUI()
	{
		if (NameLabel == null || Project == null)
		{
			return;
		}

		NameLabel.Text = Project.Name;
	}

}
