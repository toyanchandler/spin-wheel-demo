using Vertigo.Wheel.Data;
using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    public readonly struct WheelSpinResult : System.IEquatable<WheelSpinResult>
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
            Icon = slice.Reward.WheelIcon;
            Amount = reward.Amount;
            AccentColor = reward.AccentColor;
        }

        public bool Equals(WheelSpinResult other)
        {
            return SliceIndex == other.SliceIndex
                && IsBomb == other.IsBomb
                && RewardId == other.RewardId
                && DisplayName == other.DisplayName
                && Label == other.Label
                && WinLabel == other.WinLabel
                && Icon == other.Icon
                && Amount == other.Amount
                && AccentColor.Equals(other.AccentColor);
        }

        public override bool Equals(object obj)
        {
            return obj is WheelSpinResult other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = SliceIndex;
                hash = (hash * 397) ^ IsBomb.GetHashCode();
                hash = (hash * 397) ^ (RewardId != null ? RewardId.GetHashCode() : 0);
                hash = (hash * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hash = (hash * 397) ^ (Label != null ? Label.GetHashCode() : 0);
                hash = (hash * 397) ^ (WinLabel != null ? WinLabel.GetHashCode() : 0);
                hash = (hash * 397) ^ (Icon != null ? Icon.GetHashCode() : 0);
                hash = (hash * 397) ^ Amount;
                hash = (hash * 397) ^ AccentColor.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(WheelSpinResult left, WheelSpinResult right) => left.Equals(right);

        public static bool operator !=(WheelSpinResult left, WheelSpinResult right) => !left.Equals(right);
    }
}
