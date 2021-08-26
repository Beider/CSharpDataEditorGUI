using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using CSharpDataEditorDll;

public class UIManager : Node
{
    public const string DATA_OBJECT_EDITOR_PATH = "res://Scenes/Interface/ProjectEditor.tscn";
    public static UIManager Instance;

    public delegate void EventOnEditorDirty(bool dirty);
    public event EventOnEditorDirty OnEditorDirty = delegate { };

    public delegate void EventEditorShown(ConfigProjects project, ConfigEditors editor);
    public event EventEditorShown OnEditorShown = delegate { };
    public delegate void EventOnEditorSave(ConfigProjects project, ConfigEditors editor);
    public event EventOnEditorSave OnEditorSave = delegate { };
    public delegate void EventOnEditorChange(ConfigProjects project, ConfigEditors editor);
    public event EventOnEditorChange OnEditorChanged = delegate { };

    private bool SideBarVisible = true;
    public HSplitContainer SplitContainer = null;
    public Control CollapsedMenu = null;
    public SideMenu SideMenu = null;
    public Control DataContainer;
    public Button SaveButton;
    public Button SaveButtonCol;

    public bool IsAnythingChanged {get; private set;} = false;

    public IProjectEditor SettingsEditor = null;
    public List<IProjectEditor> Editors = new List<IProjectEditor>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
    }

    public void ToggleSideBarVisible()
    {
        SideBarVisible = !SideBarVisible;
        CollapsedMenu.Visible = !SideBarVisible;
        SideMenu.Visible = SideBarVisible;
        SplitContainer.Collapsed = !SideBarVisible;
    }

    public void ShowEditor(IProjectEditor editor)
    {
        // TODO: Autosave old editor if needed
        foreach (IProjectEditor pEditor in Editors)
        {
            Control editorControl = (Control)pEditor;
            editorControl.Visible = pEditor == editor;
        }
    }

    public void SaveAll()
    {
        foreach (IProjectEditor editor in Editors)
        {
            // Do not save settings with save all as it causes reload
            if (editor != SettingsEditor)
            {
                editor.Save();
            }
        }
        SetSaveButtonsState(true);
    }

    public void SetSaveButtons(Button btn1, Button btn2)
    {
        SaveButton = btn1;
        SaveButtonCol = btn2;
        SetSaveButtonsState(true);
    }

    private void SetSaveButtonsState(bool disabled)
    {
        IsAnythingChanged = !disabled;
        SaveButton.Disabled = disabled;
        SaveButtonCol.Disabled = disabled;
    }

    public void ShowSettings()
    {
        if (SettingsEditor == null)
        {
            ConfigProjects settings = Constants.GetSettingsProject();
            SettingsEditor = NewEditor(settings, 0);
            OnEditorShown(settings, settings.Editors[0]);
        }
        ShowEditor(SettingsEditor);
    }

    public static void ShowEditor(ConfigProjects project, ConfigEditors editor)
    {
        foreach (IProjectEditor pEditor in Instance.Editors)
        {
            if (pEditor.IsEditorFor(project, editor))
            {
                Instance.ShowEditor(pEditor);
                Instance.OnEditorShown(project, editor);
                return;
            }
        }
        // Create editor wrapping object
        IProjectEditor newEditor = Instance.NewEditor(project, project.Editors.IndexOf(editor));
        Instance.ShowEditor(newEditor);
        Instance.OnEditorShown(project, editor);
    }

    protected IProjectEditor NewEditor(ConfigProjects project, int editorIndex)
    {
        if (project.Editors == null || project.Editors.Count <= editorIndex)
        {
            return null;
        }
        
        PackedScene scene = ResourceLoader.Load(DATA_OBJECT_EDITOR_PATH) as PackedScene;
        Node instance = scene.Instance();
        DataContainer.AddChild(instance);
        IProjectEditor projectEditor = (IProjectEditor)instance;
        Editors.Add(projectEditor);
        projectEditor.Init(project, project.Editors[editorIndex]);

        return projectEditor;
    }

    public static void OnProjectChanged(ConfigProjects project, ConfigEditors editor)
    {
        if (project == Constants.GetSettingsProject())
        {
            return;
        }
        Instance.SetSaveButtonsState(false);
        Instance.OnEditorDirty(true);
        Instance.OnEditorChanged(project, editor);
    }

    public static void OnProjectSaved(ConfigProjects project, ConfigEditors editor)
    {
        if (project == Constants.GetSettingsProject())
        {
            Settings.ReloadConfiguration();
            return;
        }
        Instance.OnEditorSave(project, editor);
    }

    public static void LogError(string error)
    {
        GD.Print(error);
    }

}
