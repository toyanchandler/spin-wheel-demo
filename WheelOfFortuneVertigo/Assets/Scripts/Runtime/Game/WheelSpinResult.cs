using Vertigo.Wheel.Data;
using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    public readonly struct WheelSpinResult
    {
        public readonly int SliceIndex;
        public readonly bool IsBomb;
        public readonly string RewardId;
        public readonly string DisplayName;
        public readonly string Label;
        public readonly string WinLabel;
        public readonly Sprite Icon;
        public readonly int Amount;
        public readonly Color AccentColor;

        public WheelSpinResult(int sliceIndex, WheelSliceDefinition slice)
        {
            SliceIndex = sliceIndex;
            IsBomb = slice.IsBomb;
            RewardDefinition reward = slice.Reward;
            RewardId = reward.Id;
            DisplayName = reward.DisplayName;
            Label = slice.Label;
            WinLabel = reward.WinLabel;
            Icon = reward.Icon;
            Amount = reward.Amount;
            AccentColor = reward.AccentColor;
        }
    }
}
