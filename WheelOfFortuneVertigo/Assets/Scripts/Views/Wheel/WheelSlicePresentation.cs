using UnityEngine;

namespace Vertigo.Wheel.Views
{
    public readonly struct WheelSlicePresentation
    {
        public readonly Sprite Icon;
        public readonly int Amount;
        public readonly Color AmountColor;
        public readonly bool ShowAmountLabel;
        public readonly string Label;

        public WheelSlicePresentation(
            Sprite icon,
            int amount,
            Color amountColor,
            bool showAmountLabel,
            string label)
        {
            Icon = icon;
            Amount = amount;
            AmountColor = amountColor;
            ShowAmountLabel = showAmountLabel;
            Label = label;
        }
    }
}
