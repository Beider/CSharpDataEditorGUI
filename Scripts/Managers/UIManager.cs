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
    public delegate void EventOnEditorReloaded(ConfigProjects project, ConfigEditors editor);
    public event EventOnEditorReloaded OnEditorReloaded = delegate { };

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
    public Dictionary<Timer, DateTime> CommandTimers = new Dictionary<Timer, DateTime>();
    private List<string> ChangedProjects = new List<string>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
        RefreshCommandTimers();
        Settings.Instance.OnSettingsRefresh += RefreshCommandTimers;
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

    private void CloseAllEditors()
    {
        foreach (IProjectEditor editor in Editors)
        {
            if (editor.GetConfigEditor() == Constants.GetSettingsProject().Editors[0])
            {
                continue;
            }
            if (editor.GetConfigEditor().AutoSave)
            {
                editor.Save();
            }
            ((Node)editor).QueueFree();
        }

        Editors.Clear();
        ChangedProjects.Clear();
        SetSaveButtonsState(true);
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
        IProjectEditor pEditor = Instance.GetEditorFor(project, editor);
        if (pEditor != null)
        {
            Instance.ShowEditor(pEditor);
            Instance.OnEditorShown(project, editor);
            return;
        }
        // Create editor wrapping object
        IProjectEditor newEditor = Instance.NewEditor(project, project.Editors.IndexOf(editor));
        Instance.ShowEditor(newEditor);
        Instance.OnEditorShown(project, editor);
    }

    private IProjectEditor GetEditorFor(ConfigProjects project, ConfigEditors editor)
    {
        foreach (IProjectEditor pEditor in Instance.Editors)
        {
            if (pEditor.IsEditorFor(project, editor))
            {
                return pEditor;
            }
        }
        return null;
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

    private void UpdateSaveAllState(ConfigProjects project, ConfigEditors editor, bool added)
    {
        string key = project.Name+editor.Name;
        if (added && !Instance.ChangedProjects.Contains(key))
        {
            Instance.ChangedProjects.Add(key);
        }
        else if (!added && Instance.ChangedProjects.Contains(key))
        {
            Instance.ChangedProjects.Remove(key);
        }

        // Update save all state
        Instance.SetSaveButtonsState(Instance.ChangedProjects.Count == 0);
    }

    public static void OnProjectChanged(ConfigProjects project, ConfigEditors editor)
    {
        if (project == Constants.GetSettingsProject())
        {
            return;
        }
        
        Instance.UpdateSaveAllState(project, editor, true);
        Instance.OnEditorDirty(true);
        Instance.OnEditorChanged(project, editor);
    }

    public static void OnProjectSaved(ConfigProjects project, ConfigEditors editor)
    {
        if (project == Constants.GetSettingsProject())
        {
            Settings.ReloadConfiguration();
            Instance.CloseAllEditors();
            return;
        }

        Instance.UpdateSaveAllState(project, editor, false);
        Instance.OnEditorSave(project, editor);
    }

    public static void LogError(string error)
    {
        GD.Print(error);
    }

#region COMMAND TIMERS

    public static void RefreshCommandTimers()
    {
        // Free existing timers
        if (Instance.CommandTimers.Count > 0)
        {
            foreach (Timer t in Instance.CommandTimers.Keys)
            {
                t.Stop();
                t.QueueFree();
            }
            Instance.CommandTimers.Clear();
        }

        if (Settings.Instance.Configuration.Projects == null)
        {
            return;
        }

        foreach (ConfigProjects project in Settings.Instance.Configuration.Projects)
        {
            Instance.InitCommandTimer(project);
        }
    }

    private void InitCommandTimer(ConfigProjects project)
    {
        if (project.CommandFileFolder == null || project.CommandFileFolder == "")
        {
            return;
        }

        Timer commandTimer = new Timer();

        // Create parameters
        Godot.Collections.Array parameters = new Godot.Collections.Array();
        parameters.Add(commandTimer);
        parameters.Add(Settings.Instance.Configuration.Projects.IndexOf(project));
        string commandFilePath = project.CommandFileFolder;
        if (!commandFilePath.EndsWith("/") && !commandFilePath.EndsWith("\\"))
        {
            commandFilePath += "/";
        }
        commandFilePath += Constants.COMMAND_FILE_NAME;
        parameters.Add(commandFilePath);

        // Connect and start
        commandTimer.Connect("timeout", this, nameof(OnCommandTimerTimeout), parameters);
        AddChild(commandTimer);

        // Make sure we don't instantly read the file
        DateTime dateTime = new DateTime(0);
        if (System.IO.File.Exists(commandFilePath))
        {
            dateTime = System.IO.File.GetLastWriteTime(commandFilePath);
        }

        CommandTimers.Add(commandTimer, dateTime);

        float time = (float)project.CommandScanIntervalMs / 1000;

        commandTimer.WaitTime = time;
        commandTimer.Start();
    }

    private void OnCommandTimerTimeout(Timer timer, int projectIndex, string cmdFilePath)
    {
        if (System.IO.File.Exists(cmdFilePath))
        {
            DateTime modificationTime = System.IO.File.GetLastWriteTime(cmdFilePath);
            DateTime lastModTime = CommandTimers[timer];
            if (modificationTime > lastModTime)
            {
                CommandTimers[timer] = modificationTime;
                string[] commands = System.IO.File.ReadAllLines(cmdFilePath);
                ConfigProjects project = Settings.Instance.Configuration.Projects[projectIndex];
                ProcessCommands(commands, project);
            }
        }
    }

    private void ProcessCommands(string[] commands, ConfigProjects project)
    {
        try
        {
            foreach (string command in commands)
            {
                string[] cmdSplit = command.Split(Constants.COMMAND_SEPARATOR);
                foreach (ConfigEditors editor in project.Editors)
                {
                    if (cmdSplit[0].Equals(editor.CommandKey))
                    {
                        ExecuteCommand(project, editor, cmdSplit[1]);
                        break;
                    }
                }
            }
        }
        catch (Exception)
        {
            // Do nothing
        }
    }

    private void ExecuteCommand(ConfigProjects project, ConfigEditors editor, string objectName)
    {
        IProjectEditor pEditor = Instance.GetEditorFor(project, editor);
        if (pEditor == null)
        {
            pEditor = Instance.NewEditor(project, project.Editors.IndexOf(editor));
        }
        else if (editor.AutoSave)
        {
            pEditor.Save();
        }
        else
        {
            OnEditorReloaded(project, editor);
            Instance.UpdateSaveAllState(project, editor, false);
        }
        pEditor.EditObject(objectName);
        if (editor.AutoBringToFront)
        {
            Instance.ShowEditor(pEditor);
            Instance.OnEditorShown(project, editor);
        }
    }

#endregion

}
