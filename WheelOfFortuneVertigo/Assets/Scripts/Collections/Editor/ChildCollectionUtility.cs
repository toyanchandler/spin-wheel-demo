#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using Vertigo.Collections;

namespace Vertigo.Collections.Editor
{
    public static class ChildCollectionUtility
    {
        internal static void VisitOrdered(ChildCollectionSettings settings, Action<Transform> onChild)
        {
            if (settings.Scope == ChildCollectionScope.DirectChildren)
            {
                CollectDirectChildren(settings, onChild);
                return;
            }

            CollectDescendants(settings, onChild);
        }

        internal static void ApplySerializedArray<TItem>(UnityEngine.Object target, string propertyName, TItem[] values) where TItem : UnityEngine.Object
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void ApplySerializedStructArray<TEntry>(
            UnityEngine.Object target,
            string propertyName,
            TEntry[] values,
            Action<SerializedProperty, TEntry> writeEntry)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                writeEntry(property.GetArrayElementAtIndex(i), values[i]);
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CollectDirectChildren(ChildCollectionSettings settings, Action<Transform> onChild)
        {
            Transform root = settings.Root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (!ShouldInclude(child, settings)) continue;

                onChild(child);
            }
        }

        private static void CollectDescendants(ChildCollectionSettings settings, Action<Transform> onChild)
        {
            Transform root = settings.Root;
            for (int i = 0; i < root.childCount; i++)
            {
                VisitDescendant(root.GetChild(i), settings, onChild);
            }
        }

        private static void VisitDescendant(Transform node, ChildCollectionSettings settings, Action<Transform> onChild)
        {
            if (!ShouldInclude(node, settings)) return;

            onChild(node);

            for (int i = 0; i < node.childCount; i++)
            {
                VisitDescendant(node.GetChild(i), settings, onChild);
            }
        }

        private static bool ShouldInclude(Transform child, ChildCollectionSettings settings) => settings.IncludeInactive || child.gameObject.activeInHierarchy;
    }
}
#endif
