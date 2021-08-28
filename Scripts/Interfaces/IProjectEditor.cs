using System;
using CSharpDataEditorDll;
public interface IProjectEditor
{
    /// <summary>
    /// Initialize this editor
    /// </summary>
    /// <param name="project"></param>
    /// <param name="editor"></param>
    void Init(ConfigProjects project, ConfigEditors editor);

    /// <summary>
    /// Trigger save
    /// </summary>
    bool Save();

    /// <summary>
    /// Called by edit object command
    /// </summary>
    void EditObject(string objectName);

    /// <summary>
    /// Returns true if this is the editor for the given project and editor
    /// </summary>
    /// <param name="project"></param>
    /// <param name="editor"></param>
    /// <returns></returns>
    bool IsEditorFor(ConfigProjects project, ConfigEditors editor);

    ConfigEditors GetConfigEditor();
}