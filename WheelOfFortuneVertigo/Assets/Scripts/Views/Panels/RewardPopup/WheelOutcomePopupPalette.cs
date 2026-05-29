using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal static class WheelOutcomePopupPalette
    {
        public static Color FlashColor(WheelOutcomeSnapshot snapshot) => snapshot.Motion.ResolveFlashColor(snapshot.Phase);

        public static float FlashAlpha(WheelOutcomeSnapshot snapshot) => snapshot.Motion.ResolveFlashAlpha(snapshot.Phase);

        public static Color VisibleIconColor(Color color)
        {
            // Reward popup center icon must always render with its baked sprite colors.
            // Ignore the incoming tint and return opaque white so the Image acts as a neutral tint.
            return Color.white;
        }
    }
}
