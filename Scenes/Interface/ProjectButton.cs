using Godot;
using System;
using System.Reflection;
using CSharpDataEditorDll;

public class ProjectButton : Control
{
	private Button PrjButton = null;
	private Label ProjectNameLabel = null;
	private Label EditorNameLabel = null;
	private ConfigProjects Project = null;
	private ConfigEditors Editor = null;

	private Color ColorProject;
	private Color ColorEditor;
	private Control ChangeNotification;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PrjButton = FindNode("Button") as Button;
		PrjButton.Connect("pressed", this, nameof(OnButtonPressed));
		ProjectNameLabel = FindNode("ProjectName") as Label;
		EditorNameLabel = FindNode("EditorName") as Label;
		ColorProject = Utils.ResolveColorFromString(Constants.COLOR_PROJECT);
		ColorEditor = Utils.ResolveColorFromString(Constants.COLOR_EDITOR);
		ProjectNameLabel.AddColorOverride("font_color", ColorProject);
		EditorNameLabel.AddColorOverride("font_color", ColorEditor);

		ChangeNotification = FindNode("ChangeNotification") as Control;
		ChangeNotification.Visible = false;

		UIManager.Instance.OnEditorShown += OnToggleButtonClicked;
		UIManager.Instance.OnEditorChanged += OnEditorChanged;
		UIManager.Instance.OnEditorSave += OnEditorSave;
		
		UpdateUI();
	}

	public override void _ExitTree()
	{
		UIManager.Instance.OnEditorShown -= OnToggleButtonClicked;
	}

	private void OnEditorSave(ConfigProjects project, ConfigEditors editor)
	{
		bool isThis = Project == project && Editor == editor;
		if (isThis)
		{
			ChangeNotification.Visible = false;
		}
	}

	private void OnEditorChanged(ConfigProjects project, ConfigEditors editor)
	{
		bool isThis = Project == project && Editor == editor;
		if (isThis)
		{
			ChangeNotification.Visible = true;
		}
	}

	private void OnToggleButtonClicked(ConfigProjects project, ConfigEditors editor)
	{
		bool isThis = Project == project && Editor == editor;
		PrjButton.Pressed = isThis;
	}

	public void InitButton(ConfigProjects project, ConfigEditors editor)
	{
		Project = project;
		Editor = editor;

		// TODO: Validate
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
		UpdateUI();
	}

	private void OnButtonPressed()
	{
		if (Project != null)
		{
			UIManager.ShowEditor(Project, Editor);
		}
	}

	private void UpdateUI()
	{
		if (ProjectNameLabel == null || Project == null)
		{
			return;
		}

		ProjectNameLabel.Text = Project.Name;
		EditorNameLabel.Text = Editor.Name;
	}

}
