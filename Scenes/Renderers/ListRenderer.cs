using Godot;
using System;
using CSharpDataEditorDll;

public class ListRenderer : ItemList, IRenderer
{
	private TreeItem TreeItem;

	private CSDataObjectMember DataObject;

	private IDataObjectDisplay Display;

	private CSDOList Renderer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;
		Connect("focus_exited", this, nameof(OnFocusExited));
	}

	public void ShowRenderer(CSDataObjectMember dataObject, Rect2 position, IDataObjectDisplay display)
	{
		Clear();
		DataObject = dataObject;
		TreeItem = dataObject.GetMetadata<TreeItem>(Constants.METADATA_TREE_ITEM, null);
		Display = display;
		
		Renderer = dataObject.GetCustomAttribute<CSDOList>();
		string[] values = Renderer.GetList(dataObject);

		// Add the items to the list
		foreach (string value in values)
		{
			AddItem(value);
			Color color = Utils.ResolveColorFromString(Renderer.GetColor(value, dataObject));
			if (color != Colors.Transparent)
			{
				SetItemCustomFgColor(GetItemCount()-1, color);
			}
			color = Utils.ResolveColorFromString(Renderer.GetBgColor(value, dataObject));
			if (color != Colors.Transparent)
			{
				SetItemCustomBgColor(GetItemCount()-1, color);
			}
		}

		// Select the current item
		for (int i=0; i < GetItemCount(); i++)
		{
			string value = GetItemText(i);
			if (value == dataObject.CurrentValue)
			{
				Select(i);
				break;
			}
		}

		// Calculate max height
		RectGlobalPosition = position.Position;
		Control parent = GetParentControl();
		int ySizemax = (int)(parent.RectSize.y - RectPosition.y);

		// Set height
		int count = values.Length + 1;
		RectSize = new Vector2(position.Size.x, Math.Min(position.Size.y * count, ySizemax));
		
		EnsureCurrentIsVisible();
		
		Visible = true;
		GrabFocus();
		
	}

	private void OnFocusExited()
	{
		if (Visible)
		{
			if (GetSelectedItems().Length > 0)
			{
				string value = GetItemText(GetSelectedItems()[0]);
				DataObject.SetValue(value);
				Display.UpdateDataObject(DataObject, value);
			}
			QueueFree();
		}
	}
}
