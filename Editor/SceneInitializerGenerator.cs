using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;

namespace SceneInitializerGenerator
{
    class SceneInitializerGenerator
    {
        public const string CLASS_TEMPLATE =
    @"#region Using Statements
using UnityEngine;
{0}
#endregion

[ExecutionOrder(ExecutionOrderValue.Earliest)]
public class SceneInitializer : MonoBehaviour {{
    
    #region Fields
    {1}
    #endregion

    private void Awake(){{
        #region Initialization
        {2}
        #endregion
    }}
}}

";

        public const string USING_TEMPLATE = "using {0} = {1}.{2};";
        public const string FIELD_TEMPLATE = "    {0} {1};";
        public const string INITIALIZE_TEMPLATE = "        {0} = gameObject.AddComponent<{1}>();";

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void LoadScripts()
        {
            List<string>
                usingStatements = new List<string>(),
                fieldDefs = new List<string>(),
                initStatements = new List<string>();

            foreach (MonoScript script in MonoImporter.GetAllRuntimeMonoScripts())
            {
                Type type = script.GetClass();

                if (type != null)
                {
                    var attr = Attribute.GetCustomAttribute(type, typeof(AutoInitAttribute)) as AutoInitAttribute;
                    if (attr != null)
                    {
                        string _namespace = type.Namespace;
                        string className = type.Name;
                        string aliasName = "_" + className;
                        char[] charArr = className.ToCharArray();
                        charArr[0] = char.ToLower(charArr[0]);
                        string fieldName = new string(charArr);

                        if (_namespace != null && _namespace.Trim() != "")
                        {
                            usingStatements.Add(string.Format(USING_TEMPLATE, aliasName, _namespace, className));
                            fieldDefs.Add(string.Format(FIELD_TEMPLATE, aliasName, fieldName));
                            initStatements.Add(string.Format(INITIALIZE_TEMPLATE, fieldName, aliasName));
                        }
                        else
                        {
                            fieldDefs.Add(string.Format(FIELD_TEMPLATE, className, fieldName));
                            initStatements.Add(string.Format(INITIALIZE_TEMPLATE, fieldName, className));
                        }
                    }
                }
            }

            string classCode = string.Format(
                CLASS_TEMPLATE,
                string.Join("\n", usingStatements),
                string.Join("\n", fieldDefs),
                string.Join("\n", initStatements)
                );

            string classPath = Path.Combine(Application.dataPath, "UnityUtilities/Scripts/AutoGen/SceneInitializer.cs");

            AutoGenClass.Generate(classPath, classCode);
        }
    }
}