using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUtilities
{
    public static class MonobehaviourExtensions
    {
        public static T AddOrGetComponent<T>(this GameObject gameObject) where T : Component
        {
            T t = gameObject.GetComponent<T>();
            if(t == null)
            {
                t = gameObject.AddComponent<T>();
            }
            return t;
        }
    }
}