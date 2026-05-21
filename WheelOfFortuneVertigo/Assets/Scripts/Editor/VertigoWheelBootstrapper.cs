#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Vertigo.Wheel.EditorTools
{
    public static class VertigoWheelBootstrapper
    {
        [MenuItem("Vertigo Case/Rebuild Wheel Scene")]
        public static void RebuildScene()
        {
            VertigoWheelAssetPipeline.ConfigureImportedSprites();
            Material material = VertigoWheelAssetPipeline.EnsureUiMaterial();
            VertigoWheelAssetPipeline.EnsureGameSettings();
            VertigoWheelAssetPipeline.EnsureUiCopyCatalog();
            VertigoWheelAssetPipeline.EnsureSkinCatalog();
            VertigoWheelAssetPipeline.EnsureSliceLayoutCatalog();
            VertigoWheelAssetPipeline.EnsureOutcomePopupMotionCatalog();
            VertigoWheelAssetPipeline.EnsureSpinResolveCatalog();
            VertigoWheelAssetPipeline.EnsureAtlas();
            VertigoWheelProjectSetup.EnsureGameViewSizes();
            VertigoWheelProjectSetup.ConfigureAndroidProject();
            VertigoWheelSceneBuilder.CreateScene(material);
        }

        [MenuItem("Vertigo Case/Apply Android Case Setup")]
        public static void ApplyAndroidCaseSetup()
        {
            VertigoWheelAssetPipeline.ConfigureImportedSprites();
            VertigoWheelAssetPipeline.EnsureUiMaterial();
            VertigoWheelAssetPipeline.EnsureAtlas();
            VertigoWheelProjectSetup.EnsureGameViewSizes();
            VertigoWheelProjectSetup.ConfigureAndroidProject();
        }

        [MenuItem("Vertigo Case/Ensure Project Assets")]
        public static void EnsureProjectAssets()
        {
            if (!Directory.Exists(VertigoWheelPaths.DemoContentPath))
            {
                return;
            }

            VertigoWheelAssetPipeline.ConfigureImportedSprites();
            Material material = VertigoWheelAssetPipeline.EnsureUiMaterial();
            VertigoWheelAssetPipeline.EnsureGameSettings();
            VertigoWheelAssetPipeline.EnsureUiCopyCatalog();
            VertigoWheelAssetPipeline.EnsureSkinCatalog();
            VertigoWheelAssetPipeline.EnsureSliceLayoutCatalog();
            VertigoWheelAssetPipeline.EnsureOutcomePopupMotionCatalog();
            VertigoWheelAssetPipeline.EnsureSpinResolveCatalog();
            VertigoWheelAssetPipeline.EnsureAtlas();
            VertigoWheelProjectSetup.EnsureGameViewSizes();
            VertigoWheelProjectSetup.ConfigureAndroidProject();
            if (!File.Exists(VertigoWheelPaths.ScenePath))
            {
                VertigoWheelSceneBuilder.CreateScene(material);
            }
        }
    }
}
#endif
