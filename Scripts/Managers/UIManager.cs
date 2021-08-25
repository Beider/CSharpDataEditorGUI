using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using CSharpDataEditorDll;

public class UIManager : Node
{
    public const string DATA_OBJECT_TREE_SCENE_PATH = "res://Scenes/CSDataObjectTree.tscn";
    public static UIManager Instance;

    private bool SideBarVisible = true;
    public HSplitContainer SplitContainer = null;
    public Control CollapsedMenu = null;
    public SideMenu SideMenu = null;
    public Control DataContainer;
    public Button SaveButton;
    public Button SaveButtonCol;

    public CSDataObjectTree SettingsEditor = null;
    public List<CSDataObjectTree> Editors = new List<CSDataObjectTree>();

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

    public void ShowEditor(CSDataObjectTree editor)
    {
        editor.Visible = true;
    }

    public void SaveAll()
    {
        foreach (CSDataObjectTree editor in Editors)
        {
            if (editor.Save() && editor == SettingsEditor)
            {
                Settings.ReloadConfiguration();
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
        SaveButton.Disabled = disabled;
        SaveButtonCol.Disabled = disabled;
    }

    public void ShowSettings()
    {
        if (SettingsEditor == null)
        {
            SettingsEditor = NewDataObjectTree();
            IDataConverter converter = new NewtonsoftJsonConverter();
            string location = Assembly.GetExecutingAssembly().Location;
            converter.Init(Settings.SettingsLocation(), nameof(ConfigSettingsJson), location);
            SettingsEditor.InitTree(Settings.SETTINGS_FILE_NAME, converter);
        }
        ShowEditor(SettingsEditor);
    }

    protected CSDataObjectTree NewDataObjectTree()
    {
        PackedScene scene = ResourceLoader.Load(DATA_OBJECT_TREE_SCENE_PATH) as PackedScene;
        Node instance = scene.Instance();
        DataContainer.AddChild(instance);
        CSDataObjectTree editor = (CSDataObjectTree)instance;
        editor.OnChange += OnEditorChanged;
        Editors.Add(editor);

        return editor;
    }

    protected void OnEditorChanged(CSDataObjectTree editor)
    {
        SetSaveButtonsState(false);
    }

    public static void LogError(string error)
    {
        GD.Print(error);
    }

}
