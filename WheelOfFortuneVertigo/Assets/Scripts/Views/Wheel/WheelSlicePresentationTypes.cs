using UnityEngine;

namespace Vertigo.Wheel.Views
{
    /// <summary>Slice reward icon data, optionally projected into a target RectTransform space.</summary>
    public readonly struct WheelSliceIconPresentation
    {
        public readonly bool IsValid;
        public readonly Sprite Sprite;
        public readonly Color Color;
        public readonly bool HasAnchoredPosition;
        public readonly Vector2 AnchoredPosition;

        public WheelSliceIconPresentation(
            Sprite sprite,
            Color color,
            bool hasAnchoredPosition,
            Vector2 anchoredPosition)
        {
            IsValid = true;
            Sprite = sprite;
            Color = color;
            HasAnchoredPosition = hasAnchoredPosition;
            AnchoredPosition = anchoredPosition;
        }

        public static WheelSliceIconPresentation Invalid => default;
    }
}
