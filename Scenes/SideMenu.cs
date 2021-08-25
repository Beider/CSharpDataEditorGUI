using Godot;
using System;

public class SideMenu : GridContainer
{
    private const string PROJECT_BUTTON_PATH = "res://Scenes/Interface/ProjectButton.tscn";
    private PackedScene ProjectButtonScene = null;
    private Control ControlPanel = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Button btnHide = FindNode("BtnHide") as Button;
        btnHide.Connect("pressed", UIManager.Instance, nameof(UIManager.ToggleSideBarVisible));
        UIManager.Instance.SideMenu = this;
        Visible = true;

        ControlPanel = FindNode("ControlPanel") as Control;

        ProjectButtonScene = ResourceLoader.Load(PROJECT_BUTTON_PATH) as PackedScene;

        Settings.Instance.OnSettingsRefresh += OnSettingsRefresh;
        OnSettingsRefresh();
	}

    private void OnSettingsRefresh()
    {
        ClearChildren();
        foreach (ConfigProjects project in Settings.Instance.Configuration.Projects)
        {
            ProjectButton btn = ProjectButtonScene.Instance() as ProjectButton;
            btn.InitButton(project);
            AddChild(btn);
        }
    }

    private void ClearChildren()
    {
        foreach (Node child in GetChildren())
        {
            if (child != ControlPanel)
            {
                child.QueueFree();
            }
        }
    }


    public void ToggleVisible()
    {
        Visible = !Visible;
    }
}
