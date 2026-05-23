#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Vertigo.Collections.Editor;

namespace Vertigo.Wheel.EditorTools
{
    internal static class WheelEditorWiring
    {
        public static void SetReference(Object target, string propertyName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetEnum(Object target, string propertyName, int enumValue)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.enumValueIndex = enumValue;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetFloat(Object target, string propertyName, float value)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetObjectArray(Object target, string propertyName, Object[] values)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void CollectChildren(MonoBehaviour host, string arrayPropertyName, Transform rootOverride = null)
        {
            ChildCollectionEditorService.Collect(host, arrayPropertyName, rootOverride);
        }

        public static void MarkSceneDirty(Component component)
        {
            if (component == null || !component.gameObject.scene.IsValid())
            {
                return;
            }

            EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
        }
    }
}
#endif
