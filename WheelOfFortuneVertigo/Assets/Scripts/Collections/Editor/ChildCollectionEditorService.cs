#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Vertigo.Collections;

namespace Vertigo.Collections.Editor
{
    public static class ChildCollectionEditorService
    {
        public static void Collect(MonoBehaviour host, string arrayPropertyName, Transform rootOverride = null)
        {
            SerializedObject serializedObject = new SerializedObject(host);
            SerializedProperty arrayProperty = serializedObject.FindProperty(arrayPropertyName);
            if (arrayProperty == null || !arrayProperty.isArray) throw new InvalidOperationException(host.name + " is missing array property " + arrayPropertyName + ".");
            Collect(host, arrayProperty, rootOverride);
        }

        public static void Collect(MonoBehaviour host, SerializedProperty arrayProperty, Transform rootOverride = null)
        {
            FieldInfo field = FindField(host.GetType(), arrayProperty.name);
            if (field == null) throw new InvalidOperationException(host.name + " is missing field " + arrayProperty.name + ".");
            CollectChildrenAttribute attribute = field.GetCustomAttribute<CollectChildrenAttribute>();
            if (attribute == null) throw new InvalidOperationException(arrayProperty.name + " is missing [CollectChildren].");
            Type elementType = field.FieldType.GetElementType();
            if (elementType == null || !typeof(Component).IsAssignableFrom(elementType)) throw new InvalidOperationException(arrayProperty.name + " must be a Component array.");
            ChildCollectionSettings settings = ChildCollectionSettings.FromHost(host, attribute, rootOverride);
            Component[] values = CollectComponents(settings, elementType);
            arrayProperty.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            arrayProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            ChildCollectionEditorUtility.FinalizeCollect(host);
        }

        public static void Reset(MonoBehaviour host, SerializedProperty arrayProperty)
        {
            arrayProperty.arraySize = 0;
            arrayProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            ChildCollectionEditorUtility.FinalizeCollect(host);
        }

        public static void CollectAllMarkedFields(MonoBehaviour host, bool preprocessOnly)
        {
            FieldInfo[] fields = host.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            for (int i = 0; i < fields.Length; i++)
            {
                CollectChildrenAttribute attribute = fields[i].GetCustomAttribute<CollectChildrenAttribute>();
                if (attribute == null || !fields[i].FieldType.IsArray) continue;

                if (preprocessOnly && !ChildCollectionEditorPrefs.GetCollectOnPreprocessBuild(host, fields[i].Name, attribute)) continue;

                Collect(host, fields[i].Name);
            }
        }

        private static Component[] CollectComponents(ChildCollectionSettings settings, Type elementType)
        {
            var items = new List<Component>();
            ChildCollectionUtility.VisitOrdered(settings, child =>
            {
                Component component = child.GetComponent(elementType);
                if (component == null) throw new InvalidOperationException(child.name + " requires " + elementType.Name + ".");

                items.Add(component);
            });

            return items.ToArray();
        }

        private static FieldInfo FindField(Type type, string fieldName)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field != null) return field;

                type = type.BaseType;
            }

            return null;
        }
    }
}
#endif
