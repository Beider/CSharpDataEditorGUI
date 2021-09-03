using Godot;
using System;
using CSharpDataEditorDll;

public interface IRenderer
{
    void ShowRenderer(CSDataObjectMember dataObject, int coulmn, Rect2 position, IDataObjectDisplay display);
}
