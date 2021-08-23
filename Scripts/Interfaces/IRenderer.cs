using Godot;
using System;
using CSharpDataEditorDll;

public interface IRenderer
{
    void ShowRenderer(CSDataObjectMember dataObject, Rect2 position, IDataObjectDisplay display);
}
