using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal static class WheelOutcomePopupContentApplier
    {
        public static void Apply(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
        {
            ApplyOverlayTone(binding, snapshot);
            ApplyStateCopy(binding, snapshot);
            binding.Icon.ApplySprite(snapshot.Icon, WheelOutcomePopupPalette.VisibleIconColor(snapshot.IconImageColor), 1f);
        }

        private static void ApplyOverlayTone(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
        {
            binding.Root.SetOverlayAlpha(snapshot.Phase == WheelGamePhase.Bombed ? 0.56f : 0.02f);
        }

        private static void ApplyStateCopy(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
        {
            binding.ResultText.Apply(snapshot.ResultText, snapshot.ResultColor);
        }
    }
}
