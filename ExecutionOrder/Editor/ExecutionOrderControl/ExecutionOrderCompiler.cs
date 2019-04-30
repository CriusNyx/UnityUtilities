using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ExecutionOrderCompiler
{
    class ExecutionOrderCompiler
    {
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void LoadScripts()
        {
            foreach (MonoScript script in MonoImporter.GetAllRuntimeMonoScripts())
            {
                Type type = script.GetClass();
                if (type != null)
                {
                    var attr = Attribute.GetCustomAttribute(type, typeof(ExecutionOrderAttribute)) as ExecutionOrderAttribute;
                    if(attr != null)
                    {
                        int order = attr.order;
                        if (MonoImporter.GetExecutionOrder(script) != order)
                        {
                            MonoImporter.SetExecutionOrder(script, order);
                        }
                    }
                }
            }
        }
    }
}