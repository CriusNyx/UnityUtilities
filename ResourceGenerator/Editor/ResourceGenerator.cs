using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Linq;
//using static System.Environment;

class ResourceGenerator : AssetPostprocessor
{
    const string CLASS_FORMAT =
@"public static class ResourceFiles
{{
{0}
}}";

    const string FORMAT = "\tpublic static string {0} = \"{1}\";";

    private const string PATTERN = "/Resources/.*\\..*";
    private static readonly Regex REGEX = new Regex(PATTERN);
    

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        //list to store references to lines, and identifiers that have been used
        List<string> lines = new List<string>();
        List<string> idents = new List<string>();

        //Get the set of applicable files
        string[] paths = 
            Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
            .Select(x => x.Replace("\\", "/"))
            .ToArray();

        //Itterate over the files, and generate lines of code for valid files
        foreach (var path in paths)
        {
            //Validate each file
            if (ValidateFile(path))
            {
                //Get the ident and string
                (string ident, string literal) = GetIdentAndLiteralString(path);

                //check the ident list to see if this identifier has already been used
                if (!idents.Contains(ident))
                {
                    //Add the identifier to the list
                    idents.Add(ident);
                    //add the line of code to the lines
                    lines.Add(string.Format(FORMAT, ident, literal));
                }
            }
        }

        string fileText = string.Format(CLASS_FORMAT, string.Join(Environment.NewLine, lines));

        string outFilePath = Path.Combine(Application.dataPath, "UnityUtilities/ResourceGenerator/Scripts/AutoGen/ResourceFiles.cs");
        AutoGenClass.Generate(outFilePath, fileText);
    }

    private static bool ValidateFile(string path)
    {
        return REGEX.IsMatch(path) && Path.GetExtension(path) != ".meta";
    }

    private static (string ident, string literal) GetIdentAndLiteralString(string path)
    {
        path = path.Substring(0, path.LastIndexOf('.'));

        int index = path.IndexOf("Resources");
        path = path.Substring(index + "Resources".Length);
        string ident = path.Replace("/", "_").Replace(".", "_").Replace(" ", "_");
        ident = ident.Trim('_');
        path = path.Trim('/');

        return (ident, path);
    }
}