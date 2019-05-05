using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityUtilities
{
    public static class AutoGenClass
    {
        public static void Generate(string path, string classCode)
        {
            string outFileDir = Path.GetDirectoryName(path);

            if (!File.Exists(path))
            {
                Directory.CreateDirectory(outFileDir);
                File.WriteAllText(path, classCode);
                AssetDatabase.Refresh();
            }
            else
            {
                string outFileText = File.ReadAllText(path);
                if (outFileText != classCode)
                {
                    Directory.CreateDirectory(outFileDir);
                    File.WriteAllText(path, classCode);
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}