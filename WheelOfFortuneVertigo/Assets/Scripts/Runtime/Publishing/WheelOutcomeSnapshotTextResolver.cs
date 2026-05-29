using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    internal static class WheelOutcomeSnapshotTextResolver
    {
        public static string ResolveResultText(
            WheelOutcomeUiCopy outcomeCopy,
            WheelSpinResult result,
            bool hasResult,
            RewardInventory inventory,
            WheelUiCopyCatalog catalog)
        {
            string resultText = ResolveFromSource(
                outcomeCopy.ResultSource,
                outcomeCopy.ResultFallback,
                result,
                hasResult,
                inventory,
                catalog);

            ApplyRewardWinFormatting(outcomeCopy.Phase, result, hasResult, ref resultText);
            return resultText;
        }

        private static string ResolveFromSource(
            WheelOutcomeResultSource resultSource,
            string resultFallback,
            WheelSpinResult result,
            bool hasResult,
            RewardInventory inventory,
            WheelUiCopyCatalog catalog)
        {
            switch (resultSource)
            {
                case WheelOutcomeResultSource.SpinResultLabel:
                    return hasResult ? result.WinLabel : resultFallback;
                case WheelOutcomeResultSource.InventorySummary:
                    return inventory.BuildSummary(catalog.InventoryEmptySummary);
                case WheelOutcomeResultSource.StaticFallback:
                default:
                    return resultFallback;
            }
        }

        private static void ApplyRewardWinFormatting(
            WheelGamePhase phase,
            WheelSpinResult result,
            bool hasResult,
            ref string resultText)
        {
            if (phase != WheelGamePhase.Won || !hasResult || result.IsBomb)
            {
                return;
            }

            string displayName = string.IsNullOrEmpty(result.DisplayName) ? resultText : result.DisplayName;
            resultText = WheelRewardLabelFormat.Format(displayName, result.Amount, alwaysShowAmount: false);
        }
    }
}
