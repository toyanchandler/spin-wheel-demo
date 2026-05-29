#if UNITY_EDITOR
using System.IO;
using UnityEditor;

namespace Vertigo.Wheel.EditorTools
{
    public static class VertigoWheelBootstrapper
    {
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
            if (!Directory.Exists(VertigoWheelPaths.DemoContentPath)) return;
            VertigoWheelAssetPipeline.ConfigureImportedSprites();
            VertigoWheelAssetPipeline.EnsureUiMaterial();
            VertigoWheelAssetPipeline.EnsureGameSettings();
            VertigoWheelAssetPipeline.EnsureUiCopyCatalog();
            VertigoWheelAssetPipeline.EnsureSkinCatalog();
            VertigoWheelAssetPipeline.EnsureOutcomePopupMotionCatalog();
            VertigoWheelAssetPipeline.EnsureSpinResolveCatalog();
            VertigoWheelAssetPipeline.EnsureAtlas();
            VertigoWheelProjectSetup.EnsureGameViewSizes();
            VertigoWheelProjectSetup.ConfigureAndroidProject();
        }
    }
}
#endif
