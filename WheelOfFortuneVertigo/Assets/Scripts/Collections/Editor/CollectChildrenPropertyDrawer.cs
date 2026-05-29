#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Vertigo.Collections;

namespace Vertigo.Collections.Editor
{
    [CustomPropertyDrawer(typeof(CollectChildrenAttribute))]
    public sealed class CollectChildrenPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif
