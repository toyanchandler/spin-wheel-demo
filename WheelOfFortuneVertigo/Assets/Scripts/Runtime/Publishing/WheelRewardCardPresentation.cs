using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    public readonly struct WheelRewardCardPresentation
    {
        public readonly Sprite Icon;
        public readonly string TitleText;
        public readonly string AmountText;
        public readonly Color AmountColor;
        public readonly Color AccentColor;
        public readonly bool ShowAmountLabel;

        public WheelRewardCardPresentation(
            Sprite icon,
            string titleText,
            string amountText,
            Color amountColor,
            Color accentColor,
            bool showAmountLabel)
        {
            Icon = icon;
            TitleText = titleText;
            AmountText = amountText;
            AmountColor = amountColor;
            AccentColor = accentColor;
            ShowAmountLabel = showAmountLabel;
        }
    }
}
