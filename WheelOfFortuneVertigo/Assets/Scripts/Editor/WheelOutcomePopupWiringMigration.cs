#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Vertigo.Wheel.Views;

namespace Vertigo.Wheel.EditorTools
{
    /// <summary>
    /// One-time: copy refs from legacy per-child binding MonoBehaviours into flat <see cref="WheelOutcomePopupSceneWiring"/>.
    /// </summary>
    public static class WheelOutcomePopupWiringMigration
    {
        private const string ContentRootLegacy = "WheelOutcomePopupContentRootBinding";
        private const string IconLegacy = "WheelOutcomePopupIconBinding";
        private const string ResultTextLegacy = "WheelOutcomePopupResultTextBinding";
        private const string ChromeLegacy = "WheelOutcomePopupChromeBinding";
        private const string RewardBackgroundLegacy = "WheelOutcomePopupRewardBackgroundBinding";
        private const string BombShadowLegacy = "WheelOutcomePopupBombCardShadowBinding";
        private const string RetryLegacy = "WheelOutcomePopupRetryButtonBinding";
        private const string FlashLegacy = "WheelOutcomePopupFlashBinding";
        private const string ShineLegacy = "WheelOutcomePopupShineBinding";
        private const string FlightLegacy = "WheelOutcomePopupFlightIconBinding";
        private const string BurstCameraLegacy = "WheelOutcomePopupRewardBurstCameraBinding";
        private const string BurstDisplayLegacy = "WheelOutcomePopupRewardBurstDisplayBinding";
        private const string BurstParticleLegacy = "WheelOutcomePopupRewardBurstParticleBinding";
        private const string RootLegacy = "WheelOutcomePopupRootBinding";

        [MenuItem("Vertigo Case/Wiring/Migrate Outcome Popup To Flat Wiring")]
        private static void MigrateSelected()
        {
            WheelOutcomePopupBindings bindings = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<WheelOutcomePopupBindings>()
                : null;

            if (bindings == null)
            {
                Debug.LogWarning("Select a GameObject with WheelOutcomePopupBindings.");
                return;
            }

            if (!Migrate(bindings))
            {
                return;
            }

            EditorUtility.SetDirty(bindings);
            Debug.Log("Migrated " + bindings.name + " to flat WheelOutcomePopupSceneWiring.");
        }

        public static bool Migrate(WheelOutcomePopupBindings bindings)
        {
            Transform root = bindings.transform;
            var wiring = new WheelOutcomePopupSceneWiring();
            bool foundAny = false;

            MonoBehaviour rootBinding = FindLegacy(root, RootLegacy);
            if (rootBinding != null)
            {
                wiring.RootCanvas = rootBinding.GetComponent<CanvasGroup>();
                wiring.RootOverlay = rootBinding.GetComponent<Image>();
                foundAny = true;
            }

            foundAny |= TryAssignRect(root, ContentRootLegacy, rect => wiring.ContentRoot = rect);
            foundAny |= TryAssignImage(root, IconLegacy, image => wiring.Icon = image);
            foundAny |= TryAssignText(root, ResultTextLegacy, text => wiring.ResultText = text);
            foundAny |= TryAssignCanvasGroup(root, ChromeLegacy, group => wiring.Chrome = group);
            foundAny |= TryAssignGameObject(root, RewardBackgroundLegacy, go => wiring.RewardPopupBackground = go);
            foundAny |= TryAssignGameObject(root, BombShadowLegacy, go => wiring.BombCardShadow = go);
            foundAny |= TryAssignGameObject(root, RetryLegacy, go => wiring.OutcomeRetryButton = go);
            foundAny |= TryAssignImage(root, FlashLegacy, image => wiring.Flash = image);
            foundAny |= TryAssignImage(root, ShineLegacy, image => wiring.Shine = image);
            foundAny |= TryAssignImage(root, FlightLegacy, image => wiring.FlightIcon = image);
            foundAny |= TryAssignCamera(root, BurstCameraLegacy, camera => wiring.RewardBurstCamera = camera);
            foundAny |= TryAssignRawImage(root, BurstDisplayLegacy, rawImage => wiring.RewardBurstDisplay = rawImage);
            foundAny |= TryAssignParticle(root, BurstParticleLegacy, particle => wiring.RewardBurstParticle = particle);

            if (!foundAny)
            {
                Debug.LogWarning(bindings.name + " has no legacy child binding components to migrate.");
                return false;
            }

            SerializedObject serialized = new SerializedObject(bindings);
            SerializedProperty wiringProperty = serialized.FindProperty("_wiring");
            SetObjectReference(wiringProperty, nameof(WheelOutcomePopupSceneWiring.RootCanvas), wiring.RootCanvas);
            SetObjectReference(wiringProperty, nameof(WheelOutcomePopupSceneWiring.RootOverlay), wiring.RootOverlay);
            SetObjectReference(wiringProperty, nameof(WheelOutcomePopupSceneWiring.ContentRoot), wiring.ContentRoot);
            SetObjectReference(wiringProperty, nameof(WheelOutcomePopupSceneWiring.Icon), wiring.Icon);
            SetObjectReference(wiringProperty, nameof(WheelOutcomePopupSceneWiring.ResultText), wiring.ResultText);
            SetObjectReference(wiringProperty, nameof(WheelOutcomePopupSceneWiring.Chrome), wiring.Chrome);
            SetObjectReference(wiringProperty, nameof(WheelOutcomePopupSceneWiring.RewardPopupBackground), wiring.RewardPopupBackground);
            SetObjectReference(wiringProperty, nameof(WheelOutcomePopupSceneWiring.BombCardShadow), wiring.BombCardShadow);
            SetObjectReference(wiringProperty, nameof(WheelOutcomePopupSceneWiring.OutcomeRetryButton), wiring.OutcomeRetryButton);
            SetObjectReference(wiringProperty, nameof(WheelOutcomePopupSceneWiring.Flash), wiring.Flash);
            SetObjectReference(wiringProperty, nameof(WheelOutcomePopupSceneWiring.Shine), wiring.Shine);
            SetObjectReference(wiringProperty, nameof(WheelOutcomePopupSceneWiring.FlightIcon), wiring.FlightIcon);
            SetObjectReference(wiringProperty, nameof(WheelOutcomePopupSceneWiring.RewardBurstCamera), wiring.RewardBurstCamera);
            SetObjectReference(wiringProperty, nameof(WheelOutcomePopupSceneWiring.RewardBurstDisplay), wiring.RewardBurstDisplay);
            SetObjectReference(wiringProperty, nameof(WheelOutcomePopupSceneWiring.RewardBurstParticle), wiring.RewardBurstParticle);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static void SetObjectReference(SerializedProperty parent, string fieldName, Object value)
        {
            parent.FindPropertyRelative(fieldName).objectReferenceValue = value;
        }

        private static MonoBehaviour FindLegacy(Transform root, string typeName)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i].GetType().Name == typeName) return behaviours[i];
            }

            return null;
        }

        private static bool TryAssignRect(Transform root, string typeName, System.Action<RectTransform> assign)
        {
            MonoBehaviour legacy = FindLegacy(root, typeName);
            if (legacy == null) return false;
            assign(legacy.transform as RectTransform);
            return true;
        }

        private static bool TryAssignImage(Transform root, string typeName, System.Action<Image> assign)
        {
            MonoBehaviour legacy = FindLegacy(root, typeName);
            if (legacy == null) return false;
            assign(legacy.GetComponent<Image>());
            return true;
        }

        private static bool TryAssignText(Transform root, string typeName, System.Action<TextMeshProUGUI> assign)
        {
            MonoBehaviour legacy = FindLegacy(root, typeName);
            if (legacy == null) return false;
            assign(legacy.GetComponent<TextMeshProUGUI>());
            return true;
        }

        private static bool TryAssignCanvasGroup(Transform root, string typeName, System.Action<CanvasGroup> assign)
        {
            MonoBehaviour legacy = FindLegacy(root, typeName);
            if (legacy == null) return false;
            assign(legacy.GetComponent<CanvasGroup>());
            return true;
        }

        private static bool TryAssignGameObject(Transform root, string typeName, System.Action<GameObject> assign)
        {
            MonoBehaviour legacy = FindLegacy(root, typeName);
            if (legacy == null) return false;
            assign(legacy.gameObject);
            return true;
        }

        private static bool TryAssignCamera(Transform root, string typeName, System.Action<Camera> assign)
        {
            MonoBehaviour legacy = FindLegacy(root, typeName);
            if (legacy == null) return false;
            assign(legacy.GetComponent<Camera>());
            return true;
        }

        private static bool TryAssignRawImage(Transform root, string typeName, System.Action<RawImage> assign)
        {
            MonoBehaviour legacy = FindLegacy(root, typeName);
            if (legacy == null) return false;
            assign(legacy.GetComponent<RawImage>());
            return true;
        }

        private static bool TryAssignParticle(Transform root, string typeName, System.Action<ParticleSystem> assign)
        {
            MonoBehaviour legacy = FindLegacy(root, typeName);
            if (legacy == null) return false;
            assign(legacy.GetComponent<ParticleSystem>());
            return true;
        }
    }
}
#endif
