#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.EditorTools
{
    public sealed class VertigoWheelDesignerWindow : EditorWindow
    {
        private SerializedObject serializedSettings;
        private WheelGameSettings settings;

        [MenuItem("Vertigo Case/Wheel Designer")]
        public static void Open()
        {
            GetWindow<VertigoWheelDesignerWindow>("Wheel Designer");
        }

        private void OnFocus()
        {
            ResolveSettings(true);
        }

        private void OnGUI()
        {
            ResolveSettings(false);
            EditorGUILayout.LabelField("Vertigo Wheel Case", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Android target ratios: 20:9, 16:9, 4:3");

            DrawToolbar();

            if (settings == null)
            {
                EditorGUILayout.HelpBox("Open or rebuild MainScene to create game_wheel_settings.", MessageType.Warning);
                return;
            }

            DrawSettings();
            DrawSelectionButton();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Apply Android Setup"))
                {
                    VertigoWheelBootstrapper.ApplyAndroidCaseSetup();
                }

                if (GUILayout.Button("Rebuild Scene"))
                {
                    VertigoWheelBootstrapper.RebuildScene();
                    ResolveSettings(true);
                }
            }
        }

        private void DrawSettings()
        {
            serializedSettings.Update();
            DrawProperty("_sliceCount");
            DrawProperty("_safeZoneInterval");
            DrawProperty("_superZoneInterval");
            DrawProperty("_theme");
            DrawProperty("_layout");
            DrawProperty("_bombReward");
            DrawProperty("_standardRewards");
            DrawProperty("_safeRewards");
            DrawProperty("_superRewards");
            serializedSettings.ApplyModifiedProperties();
        }

        private void DrawSelectionButton()
        {
            EditorGUILayout.Space(8f);
            if (GUILayout.Button("Select Settings In Hierarchy"))
            {
                Selection.activeObject = settings.gameObject;
                EditorGUIUtility.PingObject(settings.gameObject);
            }
        }

        private void ResolveSettings(bool force)
        {
            if (!force && settings != null)
            {
                return;
            }

            settings = UnityEngine.Object.FindObjectOfType<WheelGameSettings>();
            serializedSettings = settings == null ? null : new SerializedObject(settings);
        }

        private void DrawProperty(string propertyName)
        {
            SerializedProperty property = serializedSettings.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, true);
            }
        }
    }
}
#endif
