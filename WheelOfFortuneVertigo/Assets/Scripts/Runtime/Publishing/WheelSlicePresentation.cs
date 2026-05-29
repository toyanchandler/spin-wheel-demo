using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    public readonly struct WheelSlicePresentation
    {
        public readonly Sprite Icon;
        public readonly Color AmountColor;
        public readonly bool ShowAmountLabel;
        public readonly string AmountText;

        public WheelSlicePresentation(
            Sprite icon,
            Color amountColor,
            bool showAmountLabel,
            string amountText)
        {
            Icon = icon;
            AmountColor = amountColor;
            ShowAmountLabel = showAmountLabel;
            AmountText = amountText;
        }
    }
}
