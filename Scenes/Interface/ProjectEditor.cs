using Godot;
using System;
using CSharpDataEditorDll;

public class ProjectEditor : Control, IProjectEditor
{
	private Control Toolbar;
	private Button BtnSave;
	private MenuButton BtnOpen;
	private Button BtnNew;
	private AcceptDialog SaveSettingsDialog;
	private AcceptDialog ConfirmOpenDialog;
	private AcceptDialog ConfirmNewDialog;
	private RichTextLabel NameLabel;
	private NewObjectDialog NewObjectDialog;

	private CSDataObjectTree DataObjectTree;

	private ConfigProjects Project;
	private ConfigEditors Editor;
	private IDataConverter DataConverter;
	private bool HasChanges = false;
	private string EditedItemName;
	private int OpenIndex = 0;
	private string CreateName = "";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Toolbar = FindNode("Toolbar") as Control;
		DataObjectTree = FindNode("CSDataObjectTree") as CSDataObjectTree;
		DataObjectTree.OnSave += OnEditorSaved;
		DataObjectTree.OnChange += OnEditorChanged;

		BtnSave = FindNode("BtnSave") as Button;
		BtnSave.Connect("pressed", this, nameof(SaveEditor));
		BtnSave.Disabled = true;

		BtnOpen = FindNode("BtnOpen") as MenuButton;
		BtnOpen.Connect("about_to_show", this, nameof(OnOpenAboutToShow));
		BtnOpen.GetPopup().Connect("index_pressed", this, nameof(OpenMenuPressed));

		BtnNew = FindNode("BtnNew") as Button;
		BtnNew.Connect("pressed", this, nameof(OnNewButtonPressed));

		SaveSettingsDialog = FindNode("ConfirmSave") as AcceptDialog;
		SaveSettingsDialog.Connect("confirmed", this, nameof(SaveConfirmed));

		ConfirmOpenDialog = FindNode("ConfirmOpen") as AcceptDialog;
		ConfirmOpenDialog.Connect("confirmed", this, nameof(OpenConfirmed));

		ConfirmNewDialog = FindNode("ConfirmNew") as AcceptDialog;
		ConfirmNewDialog.Connect("confirmed", this, nameof(CreateNew));

		NewObjectDialog = FindNode("NewObjectDialog") as NewObjectDialog;
		NewObjectDialog.OnEditorConfirmed += OnCreateNew;

		NameLabel = FindNode("NameLabel") as RichTextLabel;
	}

	private void OnEditorSaved()
	{
		BtnSave.Disabled = true;
		HasChanges = false;
		UIManager.OnProjectSaved(Project, Editor);
		UpdateTitle();
	}

	private void OnEditorChanged()
	{
		BtnSave.Disabled = false;
		HasChanges = true;
		UIManager.OnProjectChanged(Project, Editor);
		UpdateTitle();
	}

	private void OnNewButtonPressed()
	{
		Vector2 postion = GetLocalPosition(BtnNew.RectSize, BtnNew);
		postion.x += NewObjectDialog.RectSize.x / 2;
		Rect2 pos = new Rect2(postion, NewObjectDialog.RectSize);
		GD.Print(pos);
		NewObjectDialog.Popup_(pos);
	}

	private Vector2 GetLocalPosition(Vector2 currentPos, Control control)
	{
		if (control == this)
		{
			return currentPos;
		}
		currentPos += control.RectPosition;
		return GetLocalPosition (currentPos, (Control)control.GetParent());
	}

	private void OnCreateNew(string name)
	{
		CreateName = name;
		if (HasChanges && !Editor.AutoSave)
		{
			ConfirmNewDialog.PopupCentered();
			return;
		}
		else if (Editor.AutoSave)
		{
			DataObjectTree.Save();
		}
		CreateNew();
	}

	private void CreateNew()
	{
		if (CreateName == null || CreateName == "")
		{
			return;
		}
		DataObjectTree.InitTree(CreateName, DataConverter);
		EditedItemName = CreateName;
		OnEditorChanged();
	}

	/// <summary>
	/// Called when dropdown is about to show
	/// </summary>
	private void OnOpenAboutToShow()
	{
		PopupMenu popup = BtnOpen.GetPopup();
		// Rename all children so we avoid naming conflicts as we clear and add on the same frame
		foreach (Node child in popup.GetChildren())
		{
			child.Name += "1";
		}
		popup.Clear();

		string[] objectNames = DataConverter.GetValidObjectNames();
		if (objectNames.Length == 0)
		{
			popup.AddItem(Constants.NO_OBJECTS_FOUND);
		}
		foreach (string name in objectNames)
		{
			popup.AddItem(name);
		}
	}

	private void OpenMenuPressed(int index)
	{
		OpenIndex = index;
		if (HasChanges && !Editor.AutoSave)
		{
			ConfirmOpenDialog.PopupCentered();
			return;
		}
		else if (Editor.AutoSave)
		{
			DataObjectTree.Save();
		}
		OpenConfirmed();
	}

	private void OpenConfirmed()
	{
		PopupMenu popup = BtnOpen.GetPopup();
		string text = popup.GetItemText(OpenIndex);
		EditedItemName = text;
		DataObjectTree.InitTree(text, DataConverter);
		HasChanges = false;
		UpdateTitle();
	}


	public void Init(ConfigProjects project, ConfigEditors editor)
	{
		Project = project;
		Editor = editor;
		
		DataConverter = editor.GetDataConverter(project);
		if (DataConverter == null)
		{
			UIManager.LogError($"Data converter not found {project.Name} - {editor.Name}");
			return;
		}
		DataConverter.Init(editor.DataConverterParam, editor.DataType, project.BinaryLocation);
		string[] objectNames = DataConverter.GetValidObjectNames();
		if (objectNames.Length > 0)
		{
			EditedItemName = objectNames[0];
			DataObjectTree.InitTree(objectNames[0], DataConverter);
		}

		if (!editor.AllowCreateNew)
		{
			BtnNew.Visible = false;
		}
		if (!editor.AllowOpen)
		{
			BtnOpen.Visible = false;
		}

		UpdateTitle();
	}

	private void UpdateTitle()
	{
		string colorProject = Utils.ResolveColorFromString(Constants.COLOR_PROJECT).ToHtml();
		string colorEditor = Utils.ResolveColorFromString(Constants.COLOR_EDITOR).ToHtml();
		string bbCode = $"[center][color=#{colorProject}]{Project.Name}[/color] -";
		bbCode += $" [color=#{colorEditor}]{Editor.Name}[/color]";
		bbCode += $" ({EditedItemName})";
		if (HasChanges)
		{
			bbCode += $" [color=red]*not saved*[/color]";
		}
		bbCode += "[/center]";
		NameLabel.BbcodeText = bbCode;
	}

	/// <summary>
	/// Called from the UI
	/// </summary>
	private void SaveEditor()
	{
		// Godot detects Ctrl+S even when hitting Ctrl+Alt+S
		if (Input.IsKeyPressed((int)KeyList.Alt))
		{
			return;
		}

		// Warn if we are about to save settings and we have changes
		if (Project == Constants.GetSettingsProject() && UIManager.Instance.IsAnythingChanged)
		{
			SaveSettingsDialog.PopupCentered();
		}
		else
		{
			SaveConfirmed();
		}
	}

	/// <summary>
	/// Called from save dialog if we need confirmation
	/// </summary>
	private void SaveConfirmed()
	{
		Save();
	}

	public bool Save()
	{
		return DataObjectTree.Save();
	}

	public void EditObject(string objectName)
	{
		if (DataConverter != null)
		{
			DataObjectTree.InitTree(objectName, DataConverter);
		}
	}

	public bool IsEditorFor(ConfigProjects project, ConfigEditors editor)
	{
		return Project == project && Editor == editor;
	}

}
