using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal static class WheelOutcomePopupPalette
    {
        private static readonly Color BombFlashColor = new Color(1f, 0.22f, 0.04f, 1f);
        private static readonly Color RewardFlashColor = new Color(1f, 0.8f, 0.28f, 1f);

        public static Color FlashColor(WheelOutcomeSnapshot snapshot)
        {
            return snapshot.Phase == WheelGamePhase.Bombed ? BombFlashColor : RewardFlashColor;
        }

        public static float FlashAlpha(WheelOutcomeSnapshot snapshot)
        {
            return snapshot.Phase == WheelGamePhase.Bombed
                ? WheelOutcomePopupAnimationConfig.BombFlashAlpha
                : WheelOutcomePopupAnimationConfig.RewardFlashAlpha;
        }

        public static Color VisibleIconColor(Color color)
        {
            color.a = 1f;
            return color.maxColorComponent <= 0.02f ? Color.white : color;
        }
    }
}
