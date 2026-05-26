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
            AddOptional<WheelOutcomePopupRootBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay", ref changed);
            AddOptional<WheelOutcomePopupContentRootBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot", ref changed);
            AddOptional<WheelOutcomePopupIconBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeIconValue", ref changed);
            AddOptional<WheelOutcomePopupResultTextBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeResultValue", ref changed);
            AddOptional<WheelOutcomePopupChromeBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeRewardChrome", ref changed);
            AddOptional<WheelOutcomePopupRewardBackgroundBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeRewardChrome/OutcomeRewardPopupBg", ref changed);
            AddOptional<WheelOutcomePopupBombCardShadowBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeRewardChrome/OutcomeFailCardShadow", ref changed);
            AddOptional<WheelOutcomePopupBombWarmTopGlowBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeRewardChrome/OutcomeFailWarmTopGlow", ref changed);
            AddOptional<WheelOutcomePopupRetryButtonBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeRewardChrome/ButtonOutcomeRetry", ref changed);
            AddOptional<WheelOutcomePopupFlashBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeFlash", ref changed);
            AddOptional<WheelOutcomePopupShineBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/OutcomeShine", ref changed);
            AddOptional<WheelOutcomePopupFlightIconBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/RewardFlightIcon0", ref changed);
            AddOptional<WheelOutcomePopupRewardBurstCameraBinding>("HudCanvas/OutcomePopupController/UIParticleWorldRig/UIParticleCamera", ref changed);
            AddOptional<WheelOutcomePopupRewardBurstDisplayBinding>("HudCanvas/OutcomePopupController/OutcomeOverlay/OutcomeContentRoot/RewardBurstRenderTexture", ref changed);
            AddOptional<WheelOutcomePopupRewardBurstParticleBinding>("HudCanvas/OutcomePopupController/UIParticleWorldRig/UIParticleRewardBurst", ref changed);

            if (!changed)
            {
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("WheelOutcomePopupBindingMigration applied outcome popup scene bindings.");
        }

        private static void AddOptional<TComponent>(string path, ref bool changed) where TComponent : Component
        {
            GameObject target = FindByPath(path);
            if (target == null || target.GetComponent<TComponent>() != null)
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
