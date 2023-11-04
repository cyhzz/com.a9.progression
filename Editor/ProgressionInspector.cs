using UnityEditor;
using UnityEngine;
namespace Com.A9.Progression
{
    public class ProgressionInspector : EditorWindow
    {
        [MenuItem("Progression/Progression", false, 1)]
        public static void ShowWindow()
        {
            GetWindow(typeof(ProgressionInspector));
        }

        public void OnGUI()
        {
            GUIStyle red = new GUIStyle();
            red.normal.textColor = Color.red;
            GUIStyle green = new GUIStyle();
            green.normal.textColor = Color.green;

            foreach (var item in Progression.instance.progression)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(item.Key);
                // if (item.Value.status)
                // {
                //     EditorGUILayout.LabelField("ture", green);
                // }
                // else
                // {
                //     EditorGUILayout.LabelField("false",red);
                // }
                EditorGUILayout.EndHorizontal();
            }

        }
    }
}

