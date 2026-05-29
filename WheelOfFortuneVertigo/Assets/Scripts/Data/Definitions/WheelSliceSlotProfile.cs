using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public struct WheelSliceSlotProfile
    {
        [SerializeField] private string _label;
        [SerializeField] private int _displayAmount;
        [SerializeField] private Color _displayColor;
        [SerializeField] private bool _showAmountLabel;

        public string Label => _label;
        public int DisplayAmount => _displayAmount;
        public Color DisplayColor => _displayColor;
        public bool ShowAmountLabel => _showAmountLabel;

        public static WheelSliceSlotProfile CreateReward(string label, int displayAmount, Color displayColor, bool showAmountLabel)
        {
            return new WheelSliceSlotProfile
            {
                _label = label,
                _displayAmount = displayAmount,
                _displayColor = displayColor,
                _showAmountLabel = showAmountLabel
            };
        }

        public static WheelSliceSlotProfile CreateBomb(string label, Color displayColor)
        {
            return new WheelSliceSlotProfile
            {
                _label = label,
                _displayColor = displayColor
            };
        }
    }
}
