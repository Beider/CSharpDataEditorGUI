using Godot;
using System;
using CSharpDataEditorDll;

public interface IDataObjectDisplay
{
    void UpdateDataObject(CSDataObjectMember dataObject, int column, string newValue);
}
