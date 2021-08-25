using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using CSharpDataEditorDll;

public partial class ConfigProjects
{
    [JsonIgnore]
    public const string DEFAULT_NAME = "- Please enter a project name -";

    [JsonIgnore]
    public const string COLOR_BINARY = nameof(Colors.MediumPurple);

    public bool IsValid()
    {
        if (BinaryLocation == null || BinaryLocation == "")
        {
            UIManager.LogError($"Project {Name} BinaryLocation is empty.");
            return false;
        }
        if (!System.IO.File.Exists(BinaryLocation))
        {
            UIManager.LogError($"Project {Name} BinaryLocation file does not exist.");
            return false;
        }
        return true;
    }

    public Assembly LoadBinary()
    {
        if (BinaryLocation == null || BinaryLocation == "" ||
            !System.IO.File.Exists(BinaryLocation))
        {
            return null;
        }
        try
        {
            return Assembly.Load(System.IO.File.ReadAllBytes(BinaryLocation));
        }
        catch (Exception)
        {
            return null;
        }
    }

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

    public static bool IsCommandIntervalMsVisible(CSDataObject dataObject)
    {
        CSDataObjectMember member = (CSDataObjectMember) ((CSDataObjectClass) dataObject.Parent).FindMemberByName(nameof(CommandFileFolder));
        return member.CurrentValue != null && member.CurrentValue != "";
    }

}

public partial class ConfigEditors
{
    [JsonIgnore]
    public const string DEFAULT_NAME = "- Please Enter A Name -";

    [JsonIgnore]
    public const string DATA_CONVERTER_DEFAULT_NAME = "- Select Data Converter -";

    [JsonIgnore]
    private static List<String> InternalDataConverters = new List<string>();

    public static string[] GetDataTypeList(CSDataObject dataObject)
    {
        Assembly assembly = LoadBinary(dataObject);
        if (assembly == null)
        {
            return new string[] {$"Loading binary failed, check the binary path"};
        }

        List<string> returnList = new List<string>();
        foreach (Type type in assembly.GetTypes())
        {
            if (type.GetCustomAttribute<CSDODataClass>() != null)
            {
                returnList.Add(type.FullName);
            }
        }
        return returnList.ToArray();
        
    }

    public static string GetDataTypeColor(string value, CSDataObject dataObject)
    {
        return nameof(Colors.LightBlue);
    }

    public static bool ChildrenVisible(CSDataObject dataObject)
    {
        return !ConfigProjects.ParentMemberEqualValue((CSDataObjectClass)dataObject, nameof(Name), DEFAULT_NAME);
    }

    public static string[] GetDataConverterList(CSDataObject dataObject)
    {
        LoadInternalDataConverters();

        List<string> myList = new List<string>();
        myList.AddRange(InternalDataConverters);

        Assembly binary = LoadBinary(dataObject);
        if (binary != null)
        {
            myList.AddRange(ExtractDataConverters(binary));
        }

        // Extract data converters from the binary
        return myList.ToArray();
    }

    public static string GetDataConverterColor(string value, CSDataObject dataObject)
    {
        LoadInternalDataConverters();
        if (DATA_CONVERTER_DEFAULT_NAME.Equals(value))
        {
            return nameof(Colors.Transparent);
        }
        if (InternalDataConverters.Contains(value))
        {
            return nameof(Colors.Firebrick);
        }

        // This type is from the binary
        return ConfigProjects.COLOR_BINARY;
    }

    private static Assembly LoadBinary(CSDataObject dataObject)
    {
        CSDataObject parent = dataObject.Parent;
        CSDataObjectMember binaryLoc = null;
        while (binaryLoc == null)
        {
            // Find config projects
            if (parent is CSDataObjectClass && ((CSDataObjectClass)parent).ClassType.Name.Equals(nameof(ConfigProjects)))
            {
                binaryLoc = (CSDataObjectMember)((CSDataObjectClass)parent).FindMemberByName(nameof(ConfigProjects.BinaryLocation));
                break;
            }
            if (parent.Parent == null)
            {
                break;
            }
            parent = parent.Parent;
        }

        if (binaryLoc == null || binaryLoc.CurrentValue == null || binaryLoc.CurrentValue == "" ||
            !System.IO.File.Exists(binaryLoc.CurrentValue))
        {
            return null;
        }
        try
        {
            return Assembly.Load(System.IO.File.ReadAllBytes(binaryLoc.CurrentValue));
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static void LoadInternalDataConverters()
    {
        if (InternalDataConverters.Count == 0)
        {
            List<Assembly> assemblies = new List<Assembly>();
            assemblies.Add(Assembly.GetExecutingAssembly());
            foreach (var assemblyName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                Assembly assembly = Assembly.Load(assemblyName);
                assemblies.Add(assembly);
            }
            InternalDataConverters.Add(DATA_CONVERTER_DEFAULT_NAME);
            InternalDataConverters.AddRange(ExtractDataConverters(assemblies));
        }
    }

    private static List<string> ExtractDataConverters(List<Assembly> assemblies)
    {
        List<string> returnList = new List<string>();
        foreach (Assembly assembly in assemblies)
        {
            List<string> currentList = ExtractDataConverters(assembly);
            currentList.ForEach(val => {
                if (!returnList.Contains(val))
                {
                    returnList.Add(val);
                }
            });
        }
        return returnList;
    }

    private static List<string> ExtractDataConverters(Assembly assembly)
    {
        List<string> returnList = new List<string>();
        foreach (Type type in assembly.GetTypes())
        {
            if (typeof(IDataConverter).IsAssignableFrom(type) && !type.IsInterface)
            {
                returnList.Add(type.Name);
            }
        }

        return returnList;
    }

    public static bool IsDataReaderParamVisible(CSDataObject dataObject)
    {
        CSDataObjectMember member = (CSDataObjectMember) ((CSDataObjectClass) dataObject.Parent).FindMemberByName(nameof(DataConverter));
        return member.CurrentValue != DATA_CONVERTER_DEFAULT_NAME;
    }
}