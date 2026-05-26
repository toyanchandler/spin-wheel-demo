#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vertigo.Wheel.Views;

namespace Vertigo.Wheel.EditorTools
{
    [InitializeOnLoad]
    internal static class WheelOutcomePopupBindingMigration
    {
        static WheelOutcomePopupBindingMigration()
        {
            EditorApplication.delayCall += ApplyIfNeeded;
        }

        private static void ApplyIfNeeded()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/MainScene.unity")
            {
                return;
            }

            bool changed = false;
            Add<WheelOutcomePopupRootBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay", ref changed);
            Add<WheelOutcomePopupContentRootBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot", ref changed);
            Add<WheelOutcomePopupIconBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeIconValue", ref changed);
            Add<WheelOutcomePopupTitleTextBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeTitleValue", ref changed);
            Add<WheelOutcomePopupResultTextBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeResultValue", ref changed);
            Add<WheelOutcomePopupChromeBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeRewardChrome", ref changed);
            Add<WheelOutcomePopupBombBackgroundBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeRewardChrome/OutcomePopupBg", ref changed);
            Add<WheelOutcomePopupRewardBackgroundBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeRewardChrome/OutcomeRewardPopupBg", ref changed);
            Add<WheelOutcomePopupBombCardShadowBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeRewardChrome/OutcomeFailCardShadow", ref changed);
            Add<WheelOutcomePopupBombOuterStrokeBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeRewardChrome/OutcomeFailOuterStroke", ref changed);
            Add<WheelOutcomePopupBombWarmTopGlowBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeRewardChrome/OutcomeFailWarmTopGlow", ref changed);
            Add<WheelOutcomePopupRetryButtonBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeRewardChrome/ButtonOutcomeRetry", ref changed);
            Add<WheelOutcomePopupExitButtonBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeRewardChrome/ButtonOutcomeExit", ref changed);
            Add<WheelOutcomePopupFlashBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeFlash", ref changed);
            Add<WheelOutcomePopupShineBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeShine", ref changed);
            Add<WheelOutcomePopupFlightIconBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/RewardFlightIcon0", ref changed);
            Add<WheelOutcomePopupRewardBurstCameraBinding>("HudCanvas/OutcomePopupController/UIParticleWorldRig/UIParticleCamera", ref changed);
            Add<WheelOutcomePopupRewardBurstDisplayBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/RewardBurstRenderTexture", ref changed);
            Add<WheelOutcomePopupRewardBurstParticleBinding>("HudCanvas/OutcomePopupController/UIParticleWorldRig/UIParticleRewardBurst", ref changed);

            if (!changed)
            {
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("WheelOutcomePopupBindingMigration applied outcome popup scene bindings.");
        }

        private static void Add<TComponent>(string path, ref bool changed) where TComponent : Component
        {
            GameObject target = FindByPath(path);
            if (target == null)
            {
                throw new InvalidOperationException("Missing outcome popup hierarchy path: " + path);
            }

            if (target.GetComponent<TComponent>() != null)
            {
                return;
            }

            target.AddComponent<TComponent>();
            changed = true;
        }

        private static GameObject FindByPath(string path)
        {
            Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform transform = transforms[i];
                if (!transform.gameObject.scene.IsValid())
                {
                    continue;
                }

                if (ResolvePath(transform) == path)
                {
                    return transform.gameObject;
                }
            }

            return null;
        }

        private static string ResolvePath(Transform transform)
        {
            var stack = new System.Collections.Generic.Stack<string>();
            while (transform != null)
            {
                stack.Push(transform.name);
                transform = transform.parent;
            }

            return string.Join("/", stack.ToArray());
        }
    }
}
#endif
