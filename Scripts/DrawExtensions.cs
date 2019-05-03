using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUtilities
{
    public static class DrawExtensions
    {
        public static void DrawCross(this Vector3 point, float size, float time = -1f)
        {
            DrawCross(point, size, Color.white, time);
        }

        public static void DrawCross(this Vector3 point, float size, Color color, float time = -1f)
        {
            Debug.DrawRay(point, Vector3.left * size, color, time);
            Debug.DrawRay(point, Vector3.right * size, color, time);
            Debug.DrawRay(point, Vector3.up * size, color, time);
            Debug.DrawRay(point, Vector3.down * size, color, time);
            Debug.DrawRay(point, Vector3.forward * size, color, time);
            Debug.DrawRay(point, Vector3.back * size, color, time);
        }
    }
}