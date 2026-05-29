using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public sealed class WheelSliceDefinition
    {
        [SerializeField] private bool _isBomb;
        [SerializeField] private RewardDefinition _reward;
        [SerializeField] private Sprite _displayIcon;
        [SerializeField] private int _displayAmount;
        [SerializeField] private Color _displayColor;
        [SerializeField] private string _displayLabel;
        [SerializeField] private bool _showAmountLabel;

        public bool IsBomb => _isBomb;
        public RewardDefinition Reward => _reward;
        public Sprite DisplayIcon => _displayIcon;
        public int DisplayAmount => _displayAmount;
        public Color DisplayColor => _displayColor;
        public string DisplayLabel => _displayLabel;
        public bool ShowAmountLabel => _showAmountLabel;

        public string Label => _displayLabel;

        public void ApplySlot(
            bool isBomb,
            RewardDefinition reward,
            Sprite displayIcon,
            int displayAmount,
            Color displayColor,
            string displayLabel,
            bool showAmountLabel)
        {
            _isBomb = isBomb;
            _reward = reward;
            _displayIcon = displayIcon;
            _displayAmount = displayAmount;
            _displayColor = displayColor;
            _displayLabel = displayLabel;
            _showAmountLabel = showAmountLabel;
        }
    }
}
