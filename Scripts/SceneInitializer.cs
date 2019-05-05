using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtilities.ExecutionOrder.ExecutionOrderControl;
using UnityUtilities.DataStructures;
using System.Reflection;

[ExecutionOrder(ExecutionOrderValue.Earliest)]
public class SceneInitializer : MonoBehaviour
{
    private static Thunk<Action<GameObject>> Init = new Thunk<Action<GameObject>>(
        () =>
        {
            foreach(Assembly assem in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assem.GetType("InitializeScene");
                if(type != null)
                {
                    var method = type.GetMethod("Init");
                    if(method != null)
                    {
                        return (Action<GameObject>)Delegate.CreateDelegate(typeof(Action<GameObject>), method);
                    }
                }
            }
            return (x) => { };
        }
        );

    private void Awake()
    {
        Init.Value(gameObject);
    }
}