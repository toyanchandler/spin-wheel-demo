using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>Builds <see cref="WheelOutcomeSnapshot"/> for the result popup.</summary>
    public static class WheelOutcomeSnapshotBuilder
    {
        public static WheelOutcomeSnapshot Create(
            WheelGamePhase phase,
            WheelSpinResult result,
            bool hasResult,
            RewardInventory inventory,
            WheelGameSettings settings)
        {
            WheelOutcomeUiCopy outcomeCopy = settings.UiCopy.GetOutcomeCopy(phase);
            string resultText = WheelOutcomeSnapshotTextResolver.ResolveResultText(
                outcomeCopy,
                result,
                hasResult,
                inventory,
                settings.UiCopy);
            bool showIcon = outcomeCopy.ShowIcon && hasResult && result.Icon != null;

            return new WheelOutcomeSnapshot(
                phase,
                outcomeCopy.Title,
                resultText,
                settings.UiCopy.ResolveColor(outcomeCopy.ResultColorKey, settings.Theme),
                showIcon ? result.Icon : null,
                showIcon ? ResolveOutcomeIconColor(result, hasResult) : Color.clear,
                hasResult ? result.RewardId : string.Empty,
                hasResult ? result.DisplayName : string.Empty,
                hasResult ? result.Amount : 0,
                hasResult ? result.SliceIndex : -1,
                hasResult && showIcon,
                settings.OutcomePopupMotionCatalog.Motion);
        }

        private static Color ResolveOutcomeIconColor(WheelSpinResult result, bool hasResult)
        {
            if (!hasResult)
            {
                return Color.clear;
            }

            Color color = result.AccentColor;
            color.a = 1f;
            if (color.maxColorComponent <= 0.02f)
            {
                return Color.white;
            }

            return color;
        }
    }
}
