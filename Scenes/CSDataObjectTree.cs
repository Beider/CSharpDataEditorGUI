using Godot;
using System;
using System.Collections.Generic;
using CSharpDataEditorDll;

public class CSDataObjectTree : Tree, IDataObjectDisplay
{
    public delegate void EventOnSave();
    public event EventOnSave OnSave = delegate { };
    public delegate void EventOnChange();
    public event EventOnChange OnChange = delegate { };

    public IDataConverter Converter {get; private set;}
    private string ObjectName = "";

	private CSDataObjectClass DataObjectClass = null;
	private bool Redraw = false;

	private CSDataObjectMemberArray DragObjectParent = null;

	public override void _Ready()
	{
		Connect("item_edited", this, nameof(OnItemEdited));
		Connect("item_collapsed", this, nameof(OnTreeItemCollapsed));
		Connect("custom_popup_edited", this, nameof(OnOpenCustomEditor));
		Connect("button_pressed", this, nameof(OnButtonPressed));
	}

	public void InitTree(string name, IDataConverter converter)
	{
        Converter = converter;
		DataObjectClass = Converter.GetObject(name);
        ObjectName = name;
		Redraw = true;
	}

    public void Reload()
    {
        DataObjectClass = Converter.GetObject(ObjectName);
        Redraw = true;
    }

    public bool Save()
    {
        if (DataObjectClass == null)
        {
            return false;
        }
        if (DataObjectClass.HasChanges)
        {
            bool saveResult = Converter.SaveObject(ObjectName, DataObjectClass);
            if (saveResult)
            {
                // Maybe we can avoid this, but do it to make sure we got a clean state
                Reload();
                OnSave();
            }
            return saveResult;
        }
        return false;
    }

	public override void _PhysicsProcess(float delta)
	{
		if (Redraw)
		{
			Redraw = false;
			BuildTree();
		}
		if (!GetViewport().GuiIsDragging() && DragObjectParent != null)
		{
			CollapseChildren(DragObjectParent, false);
			DragObjectParent = null;
			GD.Print("Drag stop");
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

#region DRAG & DROP

	/// <summary>
	/// We only allow dragging of array members
	/// </summary>
	public override object GetDragData(Vector2 position)
	{
		TreeItem treeItem = GetSelected();
		string key = (string)treeItem.GetMetadata(0);
		CSDataObject dataObject = DataObjectClass.GetObjectByKey(key);

		if (dataObject.Parent == null || !(dataObject.Parent is CSDataObjectMemberArray))
		{
			return null;
		}

		DropModeFlags = (int)DropModeFlagsEnum.Inbetween;

		// Drag preview
		Label preview = new Label();
		preview.Text = treeItem.GetText(0);
		SetDragPreview(preview);

		if (Settings.CollapseOnDrag)
		{
			// Collapse all
			DragObjectParent = (CSDataObjectMemberArray)dataObject.Parent;
			CollapseChildren(DragObjectParent, true);
		}

		return treeItem;
	}

	private void CollapseChildren(CSDataObjectMemberArray dataObject, bool collapse)
	{
		foreach (CSDataObject child in dataObject.GetChildren())
		{
			TreeItem item = child.GetMetadata<TreeItem>(Constants.METADATA_TREE_ITEM, null);
			if (item != null)
			{
				if (collapse)
				{
					child.SetMetadata(Constants.METADATA_COLLAPSED_DRAG, item.Collapsed);
					item.Collapsed = true;
				}
				else
				{
					item.Collapsed = child.GetMetadata<bool>(Constants.METADATA_COLLAPSED_DRAG, false);
				}
			}
		}
	}

	/// <summary>
	/// We only allow dropping on top of the array members
	/// </summary>
	public override bool CanDropData(Vector2 position, object data)
	{
		if (!(data is TreeItem))
		{
			DropModeFlags = (int)DropModeFlagsEnum.Disabled;
			return false;
		}
		TreeItem treeItem = GetItemAtPosition(position);
		if (treeItem == null)
		{
			DropModeFlags = (int)DropModeFlagsEnum.Disabled;
			return false;
		}

		string key = (string)treeItem.GetMetadata(0);
		CSDataObject targetObject = DataObjectClass.GetObjectByKey(key);
		key = (string)((TreeItem)data).GetMetadata(0);
		CSDataObject dropObject = DataObjectClass.GetObjectByKey(key);

		if (dropObject.Parent != targetObject.Parent)
		{
			DropModeFlags = (int)DropModeFlagsEnum.Disabled;
			return false;
		}

		DropModeFlags = (int)DropModeFlagsEnum.Inbetween;
		return true;
	}

	public override void DropData(Vector2 position, object data)
	{
		TreeItem treeItem = GetItemAtPosition(position);
		string key = (string)treeItem.GetMetadata(0);
		CSDataObject targetObject = DataObjectClass.GetObjectByKey(key);
		key = (string)((TreeItem)data).GetMetadata(0);
		CSDataObject dropObject = DataObjectClass.GetObjectByKey(key);

		if (targetObject == dropObject)
		{
			return;
		}

		if (dropObject.Parent == targetObject.Parent)
		{
			int section = GetDropSectionAtPosition(position);
			CSDataObjectMemberArray array = (CSDataObjectMemberArray)dropObject.Parent;
			array.Move(dropObject.Index, targetObject.Index, section < 1 );

			// Redraw the entire parent
			TreeItem pItem = dropObject.Parent.GetMetadata<TreeItem>(Constants.METADATA_TREE_ITEM, null);
			TreeItem ppItem = dropObject.Parent.Parent.GetMetadata<TreeItem>(Constants.METADATA_TREE_ITEM, null);
			pItem.Free();
			RenderItem(dropObject.Parent, ppItem);
            OnChange();
		}
		
	}

#endregion

#region USER INTERACTION

	private void OnTreeItemCollapsed(TreeItem item)
	{
		string key = (string)item.GetMetadata(0);
		CSDataObject dataObject = DataObjectClass.GetObjectByKey(key);
		dataObject.SetMetadata(Constants.METADATA_COLLAPSED, item.Collapsed);
	}

	private void OnButtonPressed(TreeItem treeItem, int column, int id)
	{
		string key = (string)treeItem.GetMetadata(0);
		CSDataObject dataObject = DataObjectClass.GetObjectByKey(key);
		Texture buttonTexture = treeItem.GetButton(column, id);
		
		if (buttonTexture == Utils.LoadTextureFromFile(Constants.IMAGE_ADD))
		{
			CSDataObject newObject = ((CSDataObjectMemberArray)dataObject).AddNew();
			RenderItem(newObject, treeItem);
            OnChange();
		}
		else if (buttonTexture == Utils.LoadTextureFromFile(Constants.IMAGE_REMOVE))
		{
			if (!IsDeleteKeyPressed())
			{
				return;
			}
			((CSDataObjectMemberArray)dataObject.Parent).Remove(dataObject.Index);
			treeItem.Free();
            OnChange();
		}
		else if (buttonTexture == Utils.LoadTextureFromFile(Constants.IMAGE_ERROR))
		{
			if (!IsDeleteKeyPressed())
			{
				return;
			}
			CSDataObjectMember target = dataObject.GetMetadata<CSDataObjectMember>(Constants.METADATA_DISPLAY_OVERRIDE_TARGET, null);
			if (target == null)
			{
				target = (CSDataObjectMember)dataObject;
			}
			target.SetValue(target.InitialValue);
			ItemEdited(target);
            OnChange();
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
		CSDataObjectMember target = dataObject.GetMetadata<CSDataObjectMember>(Constants.METADATA_DISPLAY_OVERRIDE_TARGET, null);
		if (target == null)
		{
			target = (CSDataObjectMember)dataObject;
		}
		if (target.SetValue(newValue))
        {
		    ItemEdited(target);
		    RefreshAllVisibilityMods();
            OnChange();
        }
	}

	private void ItemEdited(CSDataObjectMember target)
	{
		TreeItem treeItem = target.GetMetadata<TreeItem>(Constants.METADATA_TREE_ITEM, null);
		if (treeItem != null)
		{
			SetItemText(treeItem, target);
			UpdateErrorState(treeItem, 1, target.CurrentError);
		}

		// Deal with display override
		CSDataObject displayOverride = target.GetMetadata<CSDataObject>(Constants.METADATA_DISPLAY_OVERRIDE, null);
		if (displayOverride != null)
		{
			TreeItem overrideItem = displayOverride.GetMetadata<TreeItem>(Constants.METADATA_TREE_ITEM, null);
			SetItemText(overrideItem, displayOverride);
			UpdateErrorState(overrideItem, 0, target.CurrentError);
		}
	}

	public void UpdateDataObject(CSDataObjectMember dataObject, string newValue)
	{
		TreeItem item = dataObject.GetMetadata<TreeItem>(Constants.METADATA_TREE_ITEM, null);
		int column = dataObject.GetMetadata<int>(Constants.METADATA_EDITABLE_COLUMN_NUM, 1);
		if (dataObject.SetValue(newValue))
        {
		    ItemEdited(dataObject);
		    RefreshAllVisibilityMods();
            OnChange();
        }
	}

	private void UpdateErrorState(TreeItem item, int column, string error)
	{
		Texture errorTexture = Utils.LoadTextureFromFile(Constants.IMAGE_ERROR);
		// Remove existing button
		EraseButton(item, errorTexture, column);

		if (error != null)
		{
			item.AddButton(column, errorTexture, -1, false, $"{error} {Constants.MESSAGE_ERROR_KEY}");
		}
	}

	private void OnOpenCustomEditor(bool arrow_clicked)
	{
		// Get the things we need
		TreeItem item = GetEdited();
		int col = GetEditedColumn();
		string key = (string)item.GetMetadata(0);
		CSDataObject dataObject = DataObjectClass.GetObjectByKey(key);
		CSDORenderer renderer = dataObject.GetCustomAttribute<CSDORenderer>();
		PackedScene scene = Settings.GetRendererScene(renderer.GetRenderType());

		// Instance the scene from the renderer
		if (scene != null)
		{
			Control instance = scene.Instance() as Control;
			AddChild(instance);
			((IRenderer)instance).ShowRenderer((CSDataObjectMember) dataObject, GetCustomPopupRect(), this);
		}
	}

	/// <summary>
	/// Refresh any data object that has a visibility modifer that changed
	/// </summary>
	private void RefreshAllVisibilityMods()
	{
		List<CSDataObject> visModObjects = DataObjectClass.GetAllWithCustomAttribute<CSDOVisibilityModifier>();

		// We collect what we need to refresh to avoid duplicates
		List<CSDataObject> objectsToRefresh = new List<CSDataObject>();

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
				dataObject.SetMetadata(Constants.METADATA_COLLAPSED, false);

				// Only collect parents once
				if (!objectsToRefresh.Contains(dataObject.Parent))
				{
					objectsToRefresh.Add(dataObject.Parent);
				}
			}
		}

		// We do this here to refresh as little as possible
		foreach (CSDataObject dataObject in objectsToRefresh)
		{
			// Get the tree item
			TreeItem parent = dataObject.GetMetadata<TreeItem>(Constants.METADATA_TREE_ITEM, null);
			
			// Refresh all children of the tree item to maintain ordering (could be more efficient)
			foreach (CSDataObject child in dataObject.GetChildren())
			{
				TreeItem childItem = child.GetMetadata<TreeItem>(Constants.METADATA_TREE_ITEM, null);
				if (childItem != null)
				{
					childItem.Free();
				}
				RenderItem(child, parent);
			}
		}
	}

	private bool AllParentChildrenVisibile(CSDataObject dataObject)
	{
		if (dataObject.GetMetadata<bool>(Constants.METADATA_VISMOD_CHILDREN, true) == false)
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
		if (dataObject.GetMetadata<bool>(Constants.METADATA_VISMOD_SELF, true))
		{
			returnItem = CreateItem(parent);
			SetItemText(returnItem, dataObject);
		}
		return returnItem;
	}

	private void RenderChildren(CSDataObject dataObject, TreeItem parent)
	{
		if (dataObject.GetMetadata<bool>(Constants.METADATA_VISMOD_CHILDREN, true))
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
			object oldValue = dataObject.GetMetadata(Constants.METADATA_VISMOD_SELF);
			bool newValue = visibilityMod.IsSelfVisible(dataObject);
			dataObject.SetMetadata(Constants.METADATA_VISMOD_SELF, newValue);
			if (oldValue == null || (bool)oldValue != newValue)
			{
				needRefresh = true;
			}

			oldValue = dataObject.GetMetadata(Constants.METADATA_VISMOD_CHILDREN);
			newValue = visibilityMod.AreChildrenVisible(dataObject);
			dataObject.SetMetadata(Constants.METADATA_VISMOD_CHILDREN, newValue);
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
		dataObject.SetMetadata(Constants.METADATA_TREE_ITEM, item);

		// SET LEFT COLUMN VALUE
		CSDODisplayNameOverride displayOverride = dataObject.GetCustomAttribute<CSDODisplayNameOverride>();
		CSDODescription description = null;
		if (displayOverride != null)
		{
			CSDataObjectMember displayOverrideMember = (CSDataObjectMember)dataObject.GetObjectByKey(displayOverride.MemberName+"/");
			string currentValue = (string)displayOverrideMember.CurrentValue;
			displayOverrideMember.SetMetadata(Constants.METADATA_DISPLAY_OVERRIDE, dataObject);
			dataObject.SetMetadata(Constants.METADATA_DISPLAY_OVERRIDE_TARGET, displayOverrideMember);
			dataObject.SetMetadata(Constants.METADATA_EDITABLE_COLUMN_NUM, 0);
			description = displayOverrideMember.GetCustomAttribute<CSDODescription>();
			SetColor(item, displayOverrideMember, 0);
			item.SetEditable(0, true);
			if (currentValue != null && currentValue != "")
			{
				item.SetText(0, currentValue);
			}
		}
		else
		{
			description = dataObject.GetCustomAttribute<CSDODescription>();
			item.SetText(0, dataObject.GetName());
		}

		// TOOLTIP
		if (description != null)
		{
			item.SetTooltip(0, description.Description);
			item.SetTooltip(1, description.Description);
		}

		// SET RIGHT COLUMN VALUE
		if (dataObject is CSDataObjectMember)
		{
			string currentValue = ((CSDataObjectMember)dataObject).CurrentValue;
			dataObject.SetMetadata(Constants.METADATA_EDITABLE_COLUMN_NUM, 1);
			SetColor(item, (CSDataObjectMember) dataObject, 1);
			item.SetEditable(1, true);
			item.SetText(1, currentValue);
		}
		if (dataObject.GetMetadata(Constants.METADATA_COLLAPSED) != null)
		{
			item.Collapsed = (bool)dataObject.GetMetadata(Constants.METADATA_COLLAPSED, false);
		}
		else
		{
			item.Collapsed = dataObject.GetCustomAttribute<CSDOStartCollapsed>() != null;
		}

		// ADD BUTTONS
		AddCollectionButtons(item, dataObject);
	}

	/// <summary>
	/// Set the item color
	/// </summary>
	/// <param name="item">The tree item</param>
	/// <param name="renderer">The renderer to use</param>
	/// <param name="dataObject">The data object we are rendering</param>
	private void SetColor(TreeItem item, CSDataObjectMember dataObject, int textColumn)
	{
		CSDORenderer renderer = dataObject.GetCustomAttribute<CSDORenderer>();
		if (renderer == null)
		{
			return;
		}
		if (renderer.GetRenderType() != null)
		{
			item.SetCellMode(textColumn, TreeItem.TreeCellMode.Custom);
		}
		Color color = Utils.ResolveColorFromString(renderer.GetColor(dataObject.CurrentValue, dataObject));
		if (color != Colors.Transparent)
		{
			item.SetCustomColor(textColumn, color);
		}

		color = Utils.ResolveColorFromString(renderer.GetBgColor(dataObject.CurrentValue, dataObject));
		if (color != Colors.Transparent)
		{
			item.SetCustomBgColor(0, color);
			item.SetCustomBgColor(1, color);
		}
	}

	private void AddCollectionButtons(TreeItem item, CSDataObject dataObject)
	{
		Texture texture;
		if (dataObject.Index >= 0)
		{
			// Remove button
			texture = Utils.LoadTextureFromFile(Constants.IMAGE_REMOVE);
			EraseButton(item, texture, 0);
			item.AddButton(0, texture, -1, false, $"Remove {Constants.MESSAGE_DELETE_KEY}");
		}

		if (dataObject is CSDataObjectMemberArray)
		{
			// Add button
			texture = Utils.LoadTextureFromFile(Constants.IMAGE_ADD);
			EraseButton(item, texture, 0);
			item.AddButton(0, texture, -1, false, "Add");
		}
	}

	private void EraseButton(TreeItem item, Texture button, int column)
	{
		for (int i=0; i < item.GetButtonCount(column); i++)
		{
			if (item.GetButton(column, i) == button)
			{
				item.EraseButton(column, i);
				break;
			}
		}
	}
#endregion

	private bool IsDeleteKeyPressed()
	{
		return Input.IsKeyPressed(Constants.KEY_DELETE);
	}

}


