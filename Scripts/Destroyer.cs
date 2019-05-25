using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    int frameCount = 0;
    int sleepFrames = 5;
    Func<bool> func;

    public static Destroyer Create(GameObject gameObject, Func<bool> func)
    {
        var output = gameObject.AddComponent<Destroyer>();
        output.func = func;
        return output;
    }

    private void Update()
    {
        frameCount++;
        if(frameCount > sleepFrames)
        {
            if(func())
            {
                Destroy(gameObject);
            }
        }
    }
}