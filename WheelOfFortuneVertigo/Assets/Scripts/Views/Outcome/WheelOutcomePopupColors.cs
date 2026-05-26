using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelOutcomePopupColors
    {
        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static Color ResolveVisibleIconColor(Color color)
        {
            color.a = 1f;
            if (color.maxColorComponent <= 0.02f)
            {
                return Color.white;
            }

            return color;
        }
    }
}
