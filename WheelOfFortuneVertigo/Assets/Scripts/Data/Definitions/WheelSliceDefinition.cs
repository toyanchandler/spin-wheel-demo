using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public sealed class WheelSliceDefinition
    {
        public bool isBomb;
        public RewardDefinition reward;

        public Sprite displayIcon;
        public int displayAmount;
        public Color displayColor;
        public string displayLabel;
        public bool showAmountLabel;

        public string Label
        {
            get
            {
                return displayLabel;
            }
        }
    }
}
