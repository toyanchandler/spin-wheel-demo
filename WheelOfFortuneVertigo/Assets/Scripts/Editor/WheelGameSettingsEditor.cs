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
        private SerializedProperty _standardRewards;
        private SerializedProperty _safeRewards;
        private SerializedProperty _superRewards;
        private SerializedProperty _uiCopy;
        private SerializedProperty _skinCatalog;
        private SerializedProperty _outcomePopupMotionCatalog;
        private SerializedProperty _spinResolveCatalog;

        private void OnEnable()
        {
            _sliceCount = serializedObject.FindProperty("_sliceCount");
            _safeZoneInterval = serializedObject.FindProperty("_safeZoneInterval");
            _superZoneInterval = serializedObject.FindProperty("_superZoneInterval");
            _standardRewards = serializedObject.FindProperty("_standardRewards");
            _safeRewards = serializedObject.FindProperty("_safeRewards");
            _superRewards = serializedObject.FindProperty("_superRewards");
            _uiCopy = serializedObject.FindProperty("_uiCopy");
            _skinCatalog = serializedObject.FindProperty("_skinCatalog");
            _outcomePopupMotionCatalog = serializedObject.FindProperty("_outcomePopupMotionCatalog");
            _spinResolveCatalog = serializedObject.FindProperty("_spinResolveCatalog");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EnsureCatalogReferences();

            EditorGUILayout.LabelField("Vertigo Wheel Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use Wheel Designer as the main editing surface. This inspector is intentionally kept as a compact summary.", MessageType.Info);

            DrawSummary();
            DrawLinkedAssetsSummary();

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Wheel Designer", GUILayout.Height(30f)))
                {
                    VertigoWheelDesignerWindow.Open();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSummary()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Summary", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Wheel slices", _sliceCount.intValue.ToString());
            EditorGUILayout.LabelField("Safe zone cadence", "Every " + _safeZoneInterval.intValue + " zones");
            EditorGUILayout.LabelField("Super zone cadence", "Every " + _superZoneInterval.intValue + " zones");
            EditorGUILayout.LabelField("Reward pool count", RewardCount().ToString());
        }

        private void DrawLinkedAssetsSummary()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Linked Assets", EditorStyles.boldLabel);
            DrawReadOnlyObject("Zone Texts", _uiCopy);
            DrawReadOnlyObject("Wheel Skins", _skinCatalog);
            DrawReadOnlyObject("Popup Motion", _outcomePopupMotionCatalog);
            DrawReadOnlyObject("Resolve Rules", _spinResolveCatalog);
        }

        private static void DrawReadOnlyObject(string label, SerializedProperty property)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label), false);
            }
        }

        private int RewardCount()
        {
            return ArraySize(_standardRewards) + ArraySize(_safeRewards) + ArraySize(_superRewards);
        }

        private static int ArraySize(SerializedProperty property)
        {
            return property == null || !property.isArray ? 0 : property.arraySize;
        }

        private void EnsureCatalogReferences()
        {
            bool dirty = false;
            if (_uiCopy.objectReferenceValue == null)
            {
                _uiCopy.objectReferenceValue = VertigoWheelAssetPipeline.EnsureUiCopyCatalog();
                dirty = true;
            }

            if (_skinCatalog.objectReferenceValue == null)
            {
                _skinCatalog.objectReferenceValue = VertigoWheelAssetPipeline.EnsureSkinCatalog();
                dirty = true;
            }

            if (_outcomePopupMotionCatalog.objectReferenceValue == null)
            {
                _outcomePopupMotionCatalog.objectReferenceValue = VertigoWheelAssetPipeline.EnsureOutcomePopupMotionCatalog();
                dirty = true;
            }

            if (_spinResolveCatalog.objectReferenceValue == null)
            {
                _spinResolveCatalog.objectReferenceValue = VertigoWheelAssetPipeline.EnsureSpinResolveCatalog();
                dirty = true;
            }

            if (dirty)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }
    }
}
#endif
