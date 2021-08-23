using Godot;
using System;
using Newtonsoft.Json;
using CSharpDataEditorDll;

public partial class ConfigProjects
{
    [JsonIgnore]
    public const string DEFAULT_NAME = "- Please enter a project name -";

    public static bool ChildrenVisible(CSDataObject dataObject)
    {
        return !ParentMemberEqualValue((CSDataObjectClass)dataObject, nameof(Name), DEFAULT_NAME);
    }

    public static bool ParentMemberEqualValue(CSDataObjectClass dataObject, string memberName, string value)
    {
        CSDataObjectMember mData = (CSDataObjectMember) dataObject.FindMemberByName(memberName);
        if (mData != null && value.Equals(mData.CurrentValue))
        {
            return true;
        }

        return false;
    }

}

public partial class ConfigEditors
{
    [JsonIgnore]
    public const string DEFAULT_NAME = "- Please Enter A Name -";

    public static bool ChildrenVisible(CSDataObject dataObject)
    {
        return !ConfigProjects.ParentMemberEqualValue((CSDataObjectClass)dataObject, nameof(Name), DEFAULT_NAME);
    }
}