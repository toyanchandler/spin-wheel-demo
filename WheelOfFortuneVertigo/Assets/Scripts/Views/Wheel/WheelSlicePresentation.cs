using UnityEngine;

namespace Vertigo.Wheel.Views
{
    public readonly struct WheelSlicePresentation
    {
        public readonly Sprite Icon;
        public readonly int Amount;
        public readonly Color AmountColor;
        public readonly Vector2 AnchoredPosition;
        public readonly bool ShowAmountLabel;

        public WheelSlicePresentation(Sprite icon, int amount, Color amountColor, Vector2 anchoredPosition, bool showAmountLabel)
        {
            Icon = icon;
            Amount = amount;
            AmountColor = amountColor;
            AnchoredPosition = anchoredPosition;
            ShowAmountLabel = showAmountLabel;
        }
    }
}
