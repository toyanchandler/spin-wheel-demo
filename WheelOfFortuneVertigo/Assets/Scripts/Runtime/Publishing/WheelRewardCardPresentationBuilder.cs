using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    public static class WheelRewardCardPresentationBuilder
    {
        public const float HaloTintWeight = 0.36f;

        public static WheelRewardCardPresentation Create(RewardInventoryEntry entry, string defaultTitle)
        {
            string title = string.IsNullOrEmpty(entry.DisplayName) ? defaultTitle : entry.DisplayName;
            bool showAmount = entry.Amount > 1;
            return new WheelRewardCardPresentation(
                entry.Icon,
                title.ToUpperInvariant(),
                showAmount ? "x" + entry.Amount : string.Empty,
                ResolveAmountColor(entry.AccentColor),
                entry.AccentColor,
                showAmount);
        }

        public static Color ResolveHaloTint(Color accentColor)
        {
            Color haloColor = Color.Lerp(Color.white, accentColor, HaloTintWeight);
            haloColor.a = 0f;
            return haloColor;
        }

        private static Color ResolveAmountColor(Color accentColor)
        {
            Color color = Color.Lerp(Color.white, accentColor, HaloTintWeight);
            color.a = 1f;
            return color;
        }
    }
}
