using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathFunctions
{
    /// <summary>
    /// Exponential Interpolation
    /// A moves e times closer to b for each unit in dt
    /// This function is asymtotic, and will never reach b, unless a = b
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="e"></param>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static float ELerp(float a, float b, float e, float dt)
    {
        float factor = Mathf.Pow(e, dt);
        float offset = b - a;
        return offset * factor + a;
    }

    /// <summary>
    /// Exponential Interpolation
    /// A moves e times closer to b for each unit in dt
    /// This function is asymtotic, and will never reach b, unless a = b
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="e"></param>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static Vector2 ELerp(Vector2 a, Vector2 b, float e, float dt)
    {
        float factor = Mathf.Pow(e, dt);
        Vector2 offset = b - a;
        return offset * factor + a;
    }

    /// <summary>
    /// Exponential Interpolation
    /// A moves e times closer to b for each unit in dt
    /// This function is asymtotic, and will never reach b, unless a = b
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="e"></param>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static Vector3 ELerp(Vector3 a, Vector3 b, float e, float dt)
    {
        float factor = Mathf.Pow(e, dt);
        Vector3 offset = b - a;
        return offset * factor + a;
    }
}