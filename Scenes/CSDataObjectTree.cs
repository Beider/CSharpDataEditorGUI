using Godot;
using System;
using System.Collections.Generic;
using CSharpDataEditorDll;

public class CSDataObjectTree : Tree
{
#region CONSTANTS
	public const string METADATA_COLLAPSED = "Collapsed";
	public const string METADATA_DISPLAY_OVERRIDE = "DisplayOverride";
	public const string METADATA_DISPLAY_OVERRIDE_TARGET = "OverrideBy";
	public const string METADATA_VISMOD_SELF = "VMSelf";
	public const string METADATA_VISMOD_CHILDREN = "VMChildren";
	public const string METADATA_TREE_ITEM = "TreeItem";
	private const string IMAGE_ADD = "res://Assets/Images/add.png";
	private const string IMAGE_REMOVE = "res://Assets/Images/remove.png";
	private const string IMAGE_ERROR = "res://Assets/Images/hazard-sign.png";
	private const string MESSAGE_DELETE_KEY = "(Hold SHIFT when pressing to delete)";
	private const string MESSAGE_ERROR_KEY = "(Hold SHIFT and click to roll back)";
	private const int KEY_DELETE = (int)KeyList.Shift;
#endregion

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

#region USER INTERACTION

	private void OnTreeItemCollapsed(TreeItem item)
	{
		string key = (string)item.GetMetadata(0);
		CSDataObject dataObject = DataObjectClass.GetObjectByKey(key);
		dataObject.SetMetadata(METADATA_COLLAPSED, item.Collapsed);
	}

	private void OnButtonPressed(TreeItem treeItem, int column, int id)
	{
		string key = (string)treeItem.GetMetadata(0);
		CSDataObject dataObject = DataObjectClass.GetObjectByKey(key);
		Texture buttonTexture = treeItem.GetButton(column, id);
		
		if (buttonTexture == Utils.LoadTextureFromFile(IMAGE_ADD))
		{
			CSDataObject newObject = ((CSDataObjectMemberArray)dataObject).AddNew();
			RenderItem(newObject, treeItem);
		}
		else if (buttonTexture == Utils.LoadTextureFromFile(IMAGE_REMOVE))
		{
			if (!IsDeleteKeyPressed())
			{
				return;
			}
			((CSDataObjectMemberArray)dataObject.Parent).Remove(dataObject.Index);
			treeItem.Free();
		}
		else if (buttonTexture == Utils.LoadTextureFromFile(IMAGE_ERROR))
		{
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
		}
		RefreshAllVisibilityMods();
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
		RefreshAllVisibilityMods();
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
		for (int i=0; i < item.GetButtonCount(column); i++)
		{
			if (item.GetButton(column, i) == errorTexture)
			{
				item.EraseButton(column, i);
				break;
			}
		}
		if (error != null)
		{
			item.AddButton(column, errorTexture, -1, false, $"{error} {MESSAGE_ERROR_KEY}");
		}
	}

    /// <summary>
    /// Refresh any data object that has a visibility modifer that changed
    /// </summary>
	private void RefreshAllVisibilityMods()
	{
		List<CSDataObject> visModObjects = DataObjectClass.GetAllWithCustomAttribute<CSDOVisibilityModifier>();

		foreach (CSDataObject dataObject in visModObjects)
		{
			if (RefreshVisibilityMod(dataObject) && dataObject.Parent != null)
			{
                // Ensure all parents children are visible, else dont refresh
                if (!AllParentChildrenVisibile(dataObject.Parent))
                {
                    continue;
                }

                // Small hack to start expanded after vis change
                dataObject.SetMetadata(METADATA_COLLAPSED, false);

				// Get the parent tree item
				TreeItem parent = dataObject.Parent.GetMetadata<TreeItem>(METADATA_TREE_ITEM, null);
                
                // Refresh all children of the parent to maintain ordering (could be more efficient)
                foreach (CSDataObject child in dataObject.Parent.GetChildren())
                {
                    TreeItem childItem = child.GetMetadata<TreeItem>(METADATA_TREE_ITEM, null);
                    if (childItem != null)
                    {
                        childItem.Free();
                    }
                    RenderItem(child, parent);
                }
			}
		}
	}

    private bool AllParentChildrenVisibile(CSDataObject dataObject)
    {
        if (dataObject.GetMetadata<bool>(METADATA_VISMOD_CHILDREN, true) == false)
        {
            return false;
        }
        if (dataObject.Parent != null)
        {
            return AllParentChildrenVisibile(dataObject.Parent);
        }
        return true;
    }

#endregion

#region RENDERING CODE

	private TreeItem RenderItem(CSDataObject dataObject, TreeItem parent)
	{
		RefreshVisibilityMod(dataObject);
		TreeItem item = RenderSelf(dataObject, parent);
		RenderChildren(dataObject, item);
		return item;
	}

    private TreeItem RenderSelf(CSDataObject dataObject, TreeItem parent)
    {
        TreeItem returnItem = parent;
        if (dataObject.GetMetadata<bool>(METADATA_VISMOD_SELF, true))
		{
			returnItem = CreateItem(parent);
			SetItemText(returnItem, dataObject);
		}
        return returnItem;
    }

    private void RenderChildren(CSDataObject dataObject, TreeItem parent)
    {
        if (dataObject.GetMetadata<bool>(METADATA_VISMOD_CHILDREN, true))
		{
			foreach (CSDataObject child in dataObject.GetChildren())
			{
				RenderItem(child, parent);
			}
		}
    }

	/// <summary>
	/// Refresh the visibility mods for this item, returns true if we need refresh
	/// </summary>
	/// <param name="dataObject">The data object</param>
	/// <returns>True if we need refresh, false if not</returns>
	private bool RefreshVisibilityMod(CSDataObject dataObject)
	{
		CSDOVisibilityModifier visibilityMod = dataObject.GetCustomAttribute<CSDOVisibilityModifier>();
		bool needRefresh = false;
		if (visibilityMod != null)
		{
			object oldValue = dataObject.GetMetadata(METADATA_VISMOD_SELF);
			bool newValue = visibilityMod.IsSelfVisible(dataObject);
			dataObject.SetMetadata(METADATA_VISMOD_SELF, newValue);
			if (oldValue == null || (bool)oldValue != newValue)
			{
				needRefresh = true;
			}

			oldValue = dataObject.GetMetadata(METADATA_VISMOD_CHILDREN);
			newValue = visibilityMod.AreChildrenVisible(dataObject);
			dataObject.SetMetadata(METADATA_VISMOD_CHILDREN, newValue);
			if (oldValue == null || (bool)oldValue != newValue)
			{
				needRefresh = true;
			}
		}

		return needRefresh;
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
        if (dataObject.GetMetadata(METADATA_COLLAPSED) != null)
        {
		    item.Collapsed = (bool)dataObject.GetMetadata(METADATA_COLLAPSED, false);
        }
        else
        {
            item.Collapsed = dataObject.GetCustomAttribute<CSDOStartCollapsed>() != null;
        }

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
			item.AddButton(0, texture, -1, false, $"Remove {MESSAGE_DELETE_KEY}");
		}

		if (dataObject is CSDataObjectMemberArray)
		{
			// Add button
			texture = Utils.LoadTextureFromFile(IMAGE_ADD);
			item.AddButton(0, texture, -1, false, "Add");
		}
	}
#endregion

	private bool IsDeleteKeyPressed()
	{
		return Input.IsKeyPressed(KEY_DELETE);
	}

}


