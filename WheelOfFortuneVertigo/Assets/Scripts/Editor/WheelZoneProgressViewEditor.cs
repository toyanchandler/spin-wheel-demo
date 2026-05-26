#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Vertigo.Wheel.Views;

namespace Vertigo.Wheel.EditorTools
{
    [CustomEditor(typeof(WheelZoneProgressView))]
    public sealed class WheelZoneProgressViewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            EditorGUILayout.Space(6f);
            if (GUILayout.Button("Collect Zone Progress Children"))
            {
                WheelZoneProgressBindingUtility.CollectIntoView((WheelZoneProgressView)target);
                serializedObject.Update();
            }

            EditorGUILayout.HelpBox(
                "Legacy flat cells are rebuilt automatically before wiring.",
                MessageType.Info);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
