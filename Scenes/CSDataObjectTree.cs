using Godot;
using System;
using CSharpDataEditorDll;

public class CSDataObjectTree : Tree
{
	public const string METADATA_COLLAPSED = "Collapsed";
	public const string METADATA_DISPLAY_OVERRIDE = "DisplayOverride";
	public const string METADATA_DISPLAY_OVERRIDE_TARGET = "OverrideBy";
	public const string METADATA_TREE_ITEM = "TreeItem";
	private const string IMAGE_ADD = "res://Assets/Images/add.png";
	private const string IMAGE_REMOVE = "res://Assets/Images/remove.png";
	private const string IMAGE_ERROR = "res://Assets/Images/hazard-sign.png";
	private const string MESSAGE_DELETE_KEY = "(Hold SHIFT when pressing to delete)";
	private const string MESSAGE_ERROR_KEY = "(Hold SHIFT and click to roll back)";
	private const int KEY_DELETE = (int)KeyList.Shift;

	private const int BUTTON_ADD_ID = 0;
	private const int BUTTON_REMOVE_ID = 1;
	private const int BUTTON_ERROR_ID = 2;

	private CSDataObjectClass DataObjectClass = null;
	private bool Redraw = false;

	public void InitTree(CSDataObjectClass rootObject)
	{
		DataObjectClass = rootObject;
		Redraw = true;
	}

	public override void _PhysicsProcess(float delta)
	{
		if (Redraw)
		{
			Redraw = false;
			BuildTree();
		}
	}

	private void BuildTree()
	{
		GD.Print("Redrawing tree");
		Clear();
		Columns = 2;
		HideRoot = true;
		if (DataObjectClass != null)
		{
			RenderItem(DataObjectClass, null);
		}
	}

	private void OnTreeItemCollapsed(TreeItem item)
	{
		string key = (string)item.GetMetadata(0);
		CSDataObject dataObject = DataObjectClass.GetObjectByKey(key);
		dataObject.SetMetadata(METADATA_COLLAPSED, item.Collapsed);
	}

	private void OnButtonPressed(object item, int column, int id)
	{
		TreeItem treeItem = (TreeItem)item;
		string key = (string)treeItem.GetMetadata(0);
		CSDataObject dataObject = DataObjectClass.GetObjectByKey(key);
		
		switch (id)
		{
			case BUTTON_ADD_ID:
				CSDataObject newObject = ((CSDataObjectMemberArray)dataObject).AddNew();
				RenderItem(newObject, treeItem);
				break;
			case BUTTON_REMOVE_ID:
				if (!IsDeleteKeyPressed())
				{
					return;
				}
				((CSDataObjectMemberArray)dataObject.Parent).Remove(dataObject.Index);
				treeItem.Free();
				break;
			case BUTTON_ERROR_ID:
				if (!IsDeleteKeyPressed())
				{
					return;
				}
				CSDataObjectMember target = dataObject.GetMetadata<CSDataObjectMember>(METADATA_DISPLAY_OVERRIDE_TARGET, null);
				if (target == null)
				{
					target = (CSDataObjectMember)dataObject;
				}
				ItemEdited(treeItem, column, target, target.InitialValue);
				break;
		}
	}

	private void OnItemEdited()
	{
		TreeItem item = GetEdited();
		int column = GetEditedColumn();

		if (item.GetCellMode(column) == TreeItem.TreeCellMode.Custom)
		{
			return;
		}

		// Update the value
		string key = (string)item.GetMetadata(0);
		CSDataObject dataObject = DataObjectClass.GetObjectByKey(key);
		string newValue = item.GetText(column);

		// Deal with target override
		CSDataObjectMember target = dataObject.GetMetadata<CSDataObjectMember>(METADATA_DISPLAY_OVERRIDE_TARGET, null);
		if (target == null)
		{
			target = (CSDataObjectMember)dataObject;
		}
		ItemEdited(item, column, target, newValue);
	}

	private void ItemEdited(TreeItem item, int column, CSDataObjectMember target, string newValue)
	{
		target.SetValue(newValue);
		item.SetText(column, newValue);
		UpdateErrorState(item, column, target.CurrentError);

		TreeItem otherItem = target.GetMetadata<TreeItem>(METADATA_TREE_ITEM, null);
		if (otherItem != null && otherItem != item)
		{
			otherItem.SetText(1, newValue);
			UpdateErrorState(otherItem, 1, target.CurrentError);
		}

		// Deal with display override
		object displayOverride = target.GetMetadata<TreeItem>(METADATA_DISPLAY_OVERRIDE, null);
		if (displayOverride != null)
		{
			((TreeItem)displayOverride).SetText(0, newValue);
			UpdateErrorState((TreeItem)displayOverride, 0, target.CurrentError);
		}
	}

	private void UpdateErrorState(TreeItem item, int column, string error)
	{
		Texture errorTexture = Utils.LoadTextureFromFile(IMAGE_ERROR);
		// Remove existing button
		for (int i=item.GetButtonCount(column); i >= 0; i--)
		{
			if (item.GetButton(column, i) == errorTexture)
			{
				item.EraseButton(column, i);
			}
		}
		if (error != null)
		{
			item.AddButton(column, errorTexture, BUTTON_ERROR_ID, false, $"{error} {MESSAGE_ERROR_KEY}");
		}
	}


	private TreeItem RenderItem(CSDataObject dataObject, TreeItem parent)
	{
		if (dataObject is CSDataObjectClass)
		{
			return RenderClassItem((CSDataObjectClass)dataObject, parent);
		}
		else if (dataObject is CSDataObjectMember)
		{
			return RenderMemberItem((CSDataObjectMember)dataObject, parent);
		}
		else if (dataObject is CSDataObjectMemberArray)
		{
			return RenderArrayItem((CSDataObjectMemberArray)dataObject, parent);
		}

		return null;
	}

	private TreeItem RenderClassItem(CSDataObjectClass dataObject, TreeItem parent)
	{
		TreeItem item = CreateItem(parent);
		SetItemText(item, dataObject);

		foreach (CSDataObject child in dataObject.ClassMembers)
		{
			RenderItem(child, item);
		}

		return item;
	}

	private TreeItem RenderMemberItem(CSDataObjectMember dataObject, TreeItem parent)
	{
		TreeItem item = CreateItem(parent);
		SetItemText(item, dataObject);
		return item;
	}

	private TreeItem RenderArrayItem(CSDataObjectMemberArray dataObject, TreeItem parent)
	{
		TreeItem item = CreateItem(parent);
		SetItemText(item, dataObject);

		foreach (int index in dataObject.GetUsedIndexes())
		{
			RenderItem(dataObject.Get(index), item);
		}
		return item;
	}

	private void SetItemText(TreeItem item, CSDataObject dataObject)
	{
		// SET METADATA KEY
		item.SetMetadata(0, dataObject.GetKey());
		dataObject.SetMetadata(METADATA_TREE_ITEM, item);

		// SET LEFT COLUMN VALUE
		CSDODisplayNameOverride displayOverride = dataObject.GetCustomAttribute<CSDODisplayNameOverride>();
		if (displayOverride != null)
		{
			CSDataObjectMember displayOverrideMember = (CSDataObjectMember)dataObject.GetObjectByKey(displayOverride.MemberName+"/");
			string currentValue = (string)displayOverrideMember.CurrentValue;
			displayOverrideMember.SetMetadata(METADATA_DISPLAY_OVERRIDE, item);
			dataObject.SetMetadata(METADATA_DISPLAY_OVERRIDE_TARGET, displayOverrideMember);
			item.SetEditable(0, true);
			if (currentValue != null && currentValue != "")
			{
				item.SetText(0, currentValue);
			}
		}
		else
		{
			item.SetText(0, dataObject.GetName());
		}

		// SET RIGHT COLUMN VALUE
		if (dataObject is CSDataObjectMember)
		{
			item.SetEditable(1, true);
			item.SetText(1, ((CSDataObjectMember)dataObject).CurrentValue);
		}
		item.Collapsed = (bool)dataObject.GetMetadata(METADATA_COLLAPSED, false);

		// ADD BUTTONS
		AddCollectionButtons(item, dataObject);
	}

	private void AddCollectionButtons(TreeItem item, CSDataObject dataObject)
	{
		Texture texture;
		if (dataObject.Index >= 0)
		{
			// Remove button
			texture = Utils.LoadTextureFromFile(IMAGE_REMOVE);
			item.AddButton(0, texture, BUTTON_REMOVE_ID, false, $"Remove {MESSAGE_DELETE_KEY}");
		}

		if (dataObject is CSDataObjectMemberArray)
		{
			// Add button
			texture = Utils.LoadTextureFromFile(IMAGE_ADD);
			item.AddButton(0, texture, BUTTON_ADD_ID, false, "Add");
		}
	}

	private bool IsDeleteKeyPressed()
	{
		return Input.IsKeyPressed(KEY_DELETE);
	}

}


