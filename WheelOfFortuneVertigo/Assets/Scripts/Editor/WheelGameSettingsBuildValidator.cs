#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.EditorTools
{
    public sealed class WheelGameSettingsBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 10;

        public void OnPreprocessBuild(BuildReport report)
        {
            WheelGameSettings settings = AssetDatabase.LoadAssetAtPath<WheelGameSettings>(VertigoWheelPaths.GameSettingsPath);
            if (settings == null) throw new BuildFailedException("Missing WheelGameSettings at " + VertigoWheelPaths.GameSettingsPath);
            ValidateSettings(settings);

            WheelOutcomePopupMotionCatalog motionCatalog = settings.OutcomePopupMotionCatalog;
            if (motionCatalog == null) throw new BuildFailedException("WheelGameSettings is missing WheelOutcomePopupMotionCatalog.");
            WheelOutcomePopupMotion motion = motionCatalog.Motion;
            if (motion.BombFlashAlpha <= 0f || motion.RewardFlashAlpha <= 0f) throw new BuildFailedException(
                    "WheelOutcomePopupMotionCatalog has invalid flash alpha. Reset the catalog to defaults.");
        }

        private static void ValidateSettings(WheelGameSettings settings)
        {
            settings.InitializeRuntime();
        }
    }
}
#endif
