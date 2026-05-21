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

        public bool IsBomb { get { return _isBomb; } }
        public RewardDefinition Reward { get { return _reward; } }
        public Sprite DisplayIcon { get { return _displayIcon; } }
        public int DisplayAmount { get { return _displayAmount; } }
        public Color DisplayColor { get { return _displayColor; } }
        public string DisplayLabel { get { return _displayLabel; } }
        public bool ShowAmountLabel { get { return _showAmountLabel; } }

        public string Label { get { return _displayLabel; } }

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
