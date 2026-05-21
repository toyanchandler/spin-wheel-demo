#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.EditorTools
{
    [CustomEditor(typeof(WheelUiCopyCatalog))]
    public sealed class WheelUiCopyCatalogEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (GUILayout.Button("Reset To Defaults"))
            {
                var catalog = (WheelUiCopyCatalog)target;
                Undo.RecordObject(catalog, "Reset Wheel UI Copy Catalog");
                catalog.ResetToDefaults();
                EditorUtility.SetDirty(catalog);
            }
        }
    }
}
#endif
