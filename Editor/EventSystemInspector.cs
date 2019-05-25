using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace UnityUtilities
{
    [CustomEditor(typeof(CEventSystem))]
    public class EventSystemInspector : Editor
    {
        bool showListeners = false;
        bool showBroadcastRecords = false;
        bool repaint = true;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            showListeners = GUILayout.Toggle(showListeners, "Show Listeners", "Button");
            if (showListeners)
            {
                var listeners = CEventSystem.GetEventListeners();
                if (listeners != null)
                {
                    GUILayout.Label("Event Listeners");
                    foreach (var dic in listeners)
                    {
                        GUILayout.Label("\t" + dic.Key.ToString());
                        foreach (var dic2 in dic.Value)
                        {
                            GUILayout.Label("\t\t" + dic2.Key.ToString());
                            foreach (var listener in dic2.Value)
                            {
                                if(listener is MonoBehaviour mono)
                                {
                                    GUILayout.Label("\t\t\t" + mono.gameObject.name + "." + listener.ToString());
                                }
                                else
                                {
                                    GUILayout.Label("\t\t\t" + listener.ToString());
                                }
                                
                            }
                        }
                    }
                }
                else
                {
                    GUILayout.Label("CEventSystem returned a null list");
                }
            }

            showBroadcastRecords = GUILayout.Toggle(showBroadcastRecords, "Show Events", "Button");
            if (showBroadcastRecords)
            {
                GUILayout.BeginHorizontal();
                {
                    CEventSystem.maxQueueLength = EditorGUILayout.IntField(CEventSystem.maxQueueLength);
                    CEventSystem.maxQueueLength = Mathf.Max(120, CEventSystem.maxQueueLength);
                }
                GUILayout.EndHorizontal();

                var events = CEventSystem.GetBroadcastList();
                events = events.Reverse();
                if (events != null)
                {
                    GUILayout.Label("Boradcast Records");
                    foreach (var e in events)
                    {
                        GUILayout.Label("\t" + e.ToString());
                    }
                }
                else
                {
                    GUILayout.Label("CEventSystem returned a null list");
                }
            }

            repaint = GUILayout.Toggle(repaint, "Repaint", "Button");
            if (repaint)
            {
                Repaint();
            }
        }
    }
}