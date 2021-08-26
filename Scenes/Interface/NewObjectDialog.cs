using Godot;
using System;

public class NewObjectDialog : PopupDialog
{
    public delegate void EventEditorConfirmed(string name);
    public event EventEditorConfirmed OnEditorConfirmed = delegate { };

	private LineEdit TextInput;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TextInput = FindNode("LineEdit") as LineEdit;
	}

    private void OnAboutToShow()
    {
        TextInput.Text = "";
    }

    private void OnCreatePressed()
    {
        OnEditorConfirmed(TextInput.Text);
        Hide();
    }


    private void OnCancelPressed()
    {
        Hide();
    }
}
