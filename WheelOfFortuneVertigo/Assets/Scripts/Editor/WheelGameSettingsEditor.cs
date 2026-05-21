#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.EditorTools
{
    [CustomEditor(typeof(WheelGameSettings))]
    public sealed class WheelGameSettingsEditor : Editor
    {
        private SerializedProperty _sliceCount;
        private SerializedProperty _safeZoneInterval;
        private SerializedProperty _superZoneInterval;
        private SerializedProperty _bombReward;
        private SerializedProperty _standardRewards;
        private SerializedProperty _safeRewards;
        private SerializedProperty _superRewards;
        private SerializedProperty _uiCopy;
        private SerializedProperty _layout;
        private SerializedProperty _theme;

        private void OnEnable()
        {
            _sliceCount = serializedObject.FindProperty("_sliceCount");
            _safeZoneInterval = serializedObject.FindProperty("_safeZoneInterval");
            _superZoneInterval = serializedObject.FindProperty("_superZoneInterval");
            _bombReward = serializedObject.FindProperty("_bombReward");
            _standardRewards = serializedObject.FindProperty("_standardRewards");
            _safeRewards = serializedObject.FindProperty("_safeRewards");
            _superRewards = serializedObject.FindProperty("_superRewards");
            _uiCopy = serializedObject.FindProperty("_uiCopy");
            _layout = serializedObject.FindProperty("_layout");
            _theme = serializedObject.FindProperty("_theme");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EnsureUiCopyReference();

            EditorGUILayout.LabelField("Vertigo Wheel Designer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Scene-bound gameplay settings. UI copy lives in the WheelUiCopyCatalog ScriptableObject referenced below.", MessageType.Info);

            DrawSection("Zone Rules", _sliceCount, _safeZoneInterval, _superZoneInterval);
            DrawSection("UI Copy", _uiCopy);
            DrawSection("Theme", _theme);
            DrawSection("Layout", _layout);
            DrawSection("Bomb", _bombReward);
            DrawSection("Standard Zone Rewards", _standardRewards);
            DrawSection("Safe Zone Rewards", _safeRewards);
            DrawSection("Super Zone Rewards", _superRewards);

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Designer"))
                {
                    VertigoWheelDesignerWindow.Open();
                }

                if (GUILayout.Button("Rebuild Scene"))
                {
                    VertigoWheelBootstrapper.RebuildScene();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void EnsureUiCopyReference()
        {
            if (_uiCopy.objectReferenceValue != null)
            {
                return;
            }

            WheelUiCopyCatalog catalog = AssetDatabase.LoadAssetAtPath<WheelUiCopyCatalog>(VertigoWheelPaths.UiCopyCatalogPath);
            if (catalog == null)
            {
                catalog = VertigoWheelAssetPipeline.EnsureUiCopyCatalog();
            }

            _uiCopy.objectReferenceValue = catalog;
            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawSection(string title, params SerializedProperty[] properties)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            for (int i = 0; i < properties.Length; i++)
            {
                EditorGUILayout.PropertyField(properties[i], true);
            }
        }
    }
}
#endif
