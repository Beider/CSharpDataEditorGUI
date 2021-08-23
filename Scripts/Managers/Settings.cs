using Godot;
using System;
using System.Collections.Generic;
using CSharpDataEditorDll;

public class Settings : Node
{
    public static Settings Instance;
    private Dictionary<string, PackedScene> RendererTypes = new Dictionary<string, PackedScene>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
        InitRenderers();
    }

    private void InitRenderers()
    {
        //TODO: Resolve the packed scene for the renderer
        RendererTypes.Add(CSDOList.LIST_RENDERER_TYPE, null);
    }

    /// <summary>
    /// Get the scene for the renderer type
    /// </summary>
    /// <param name="rendererType">The type of renderer</param>
    /// <returns>The renderer scene or null if not found</returns>
    public PackedScene GetRendererScene(string rendererType)
    {
        if (RendererTypes.ContainsKey(rendererType))
        {
            return RendererTypes[rendererType];
        }
        return null;
    }

}
