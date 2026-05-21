#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Vertigo.Wheel.EditorTools
{
    [InitializeOnLoad]
    public sealed class WheelSceneValidationGate : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return -1000; } }

        static WheelSceneValidationGate()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            try
            {
                ValidateLoadedScene("pre-build");
            }
            catch (Exception exception)
            {
                throw new BuildFailedException(exception.Message);
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            try
            {
                ValidateLoadedScene("pre-play");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.isPlaying = false;
            }
        }

        private static void ValidateLoadedScene(string stage)
        {
            WheelHierarchyWiringValidator.ValidateScene();
            Debug.Log("Vertigo " + stage + " scene wiring validation passed.");
        }
    }
}
#endif
