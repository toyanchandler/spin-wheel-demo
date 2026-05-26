#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Vertigo.Collections;
using Vertigo.Collections.Editor;
using Vertigo.Wheel.Views;

namespace Vertigo.Wheel.EditorTools
{
    public class CollectChildrenComponentEditor : UnityEditor.Editor
    {
        private const string HelpText =
            "Editor helper: fills marked arrays from child objects in hierarchy order.\n\n" +
            "Collect Children - scan and assign components now.\n" +
            "Reset Children - clear the array.\n\n" +
            "Runtime uses only serialized arrays; no hierarchy search.";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawCollectionControls();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCollectionControls()
        {
            List<FieldInfo> fields = FindCollectChildrenFields(target.GetType());
            if (fields.Count == 0)
            {
                return;
            }

            ChildCollectionEditorGui.DrawSectionHeader();

            for (int i = 0; i < fields.Count; i++)
            {
                DrawFieldControls(fields[i]);
            }

            ChildCollectionEditorPrefs.HelpFoldout = ChildCollectionEditorGui.DrawInfoFoldout(
                ChildCollectionEditorPrefs.HelpFoldout,
                HelpText);

            EditorGUILayout.Space(6f);
        }

        private void DrawFieldControls(FieldInfo field)
        {
            var property = serializedObject.FindProperty(field.Name);
            if (property == null)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(field.Name), EditorStyles.miniBoldLabel);

            EditorGUI.BeginDisabledGroup(targets.Length != 1);
            ChildCollectionEditorGui.DrawActionButtons(out bool collectClicked, out bool resetClicked);
            if (collectClicked)
            {
                ChildCollectionEditorService.Collect((MonoBehaviour)target, property);
            }

            if (resetClicked)
            {
                ChildCollectionEditorService.Reset((MonoBehaviour)target, property);
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }

        private static List<FieldInfo> FindCollectChildrenFields(System.Type type)
        {
            var fields = new List<FieldInfo>();
            while (type != null)
            {
                FieldInfo[] declaredFields = type.GetFields(
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
                for (int i = 0; i < declaredFields.Length; i++)
                {
                    if (declaredFields[i].GetCustomAttribute<CollectChildrenAttribute>() != null)
                    {
                        fields.Add(declaredFields[i]);
                    }
                }

                type = type.BaseType;
            }

            fields.Reverse();
            return fields;
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(WheelView), true)]
    public sealed class WheelViewEditor : CollectChildrenComponentEditor
    {
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(WheelRewardPanelView), true)]
    public sealed class WheelRewardPanelViewEditor : CollectChildrenComponentEditor
    {
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(WheelRewardOpeningBindings), true)]
    public sealed class WheelRewardOpeningBindingsEditor : CollectChildrenComponentEditor
    {
    }
}
#endif
