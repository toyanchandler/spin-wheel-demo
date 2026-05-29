using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    public readonly struct RewardInventoryEntry
    {
        public readonly string RewardId;
        public readonly string DisplayName;
        public readonly int Amount;
        public readonly Sprite Icon;
        public readonly Color AccentColor;

        public RewardInventoryEntry(string rewardId, string displayName, int amount, Sprite icon, Color accentColor)
        {
            RewardId = rewardId;
            DisplayName = displayName;
            Amount = amount;
            Icon = icon;
            AccentColor = accentColor;
        }
    }

    public sealed class RewardInventory
    {
        private readonly Dictionary<string, RewardStack> _rewards = new Dictionary<string, RewardStack>();
        private readonly List<string> _rewardOrder = new List<string>();
        private readonly StringBuilder _builder = new StringBuilder(128);
        private string _summary = string.Empty;
        private bool _dirty;

        public int Count => _rewards.Count;

        public void Clear()
        {
            _rewards.Clear();
            _rewardOrder.Clear();
            _summary = string.Empty;
            _dirty = false;
        }

        public void Add(WheelSpinResult result, string fallbackRewardName)
        {
            bool alreadyTracked = _rewards.ContainsKey(result.RewardId);
            RewardStack stack = alreadyTracked
                ? _rewards[result.RewardId]
                : CreateStack(result, fallbackRewardName);
            stack.RefreshPresentation(result);
            stack.AddAmount(result.Amount);
            MoveRewardToNewest(result.RewardId, alreadyTracked);
            _dirty = true;
        }

        public int CopyEntries(RewardInventoryEntry[] buffer)
        {
            int index = 0;
            for (int i = 0; i < _rewardOrder.Count; i++)
            {
                if (index >= buffer.Length) break;
                string rewardId = _rewardOrder[i];
                RewardStack stack;
                if (!_rewards.TryGetValue(rewardId, out stack)) continue;
                buffer[index] = stack.ToEntry(rewardId);
                index++;
            }

            return index;
        }

        public string BuildSummary(string emptySummary)
        {
            if (!_dirty) return string.IsNullOrEmpty(_summary) ? emptySummary : _summary;
            return RebuildSummary(emptySummary);
        }

        private RewardStack CreateStack(WheelSpinResult result, string fallbackRewardName)
        {
            RewardStack stack = new RewardStack(result, fallbackRewardName);
            _rewards.Add(result.RewardId, stack);
            return stack;
        }

        private void MoveRewardToNewest(string rewardId, bool alreadyTracked)
        {
            if (alreadyTracked) _rewardOrder.Remove(rewardId);
            _rewardOrder.Add(rewardId);
        }

        private string RebuildSummary(string emptySummary)
        {
            _builder.Clear();
            int index = 0;
            for (int i = 0; i < _rewardOrder.Count; i++)
            {
                string rewardId = _rewardOrder[i];
                RewardStack stack;
                if (!_rewards.TryGetValue(rewardId, out stack)) continue;
                if (index > 0) _builder.Append("  ");
                WheelRewardLabelFormat.AppendTo(_builder, stack.DisplayName, stack.Amount, alwaysShowAmount: true);
                index++;
            }

            _summary = _builder.Length == 0 ? emptySummary : _builder.ToString();
            _dirty = false;
            return _summary;
        }

        private sealed class RewardStack
        {
            public readonly string DisplayName;
            public Sprite Icon { get; private set; }
            public readonly Color AccentColor;
            public int Amount { get; private set; }

            public RewardStack(WheelSpinResult result, string fallbackRewardName)
            {
                DisplayName = string.IsNullOrEmpty(result.DisplayName)
                    ? fallbackRewardName
                    : result.DisplayName;
                Icon = result.Icon;
                AccentColor = result.AccentColor;
                Amount = 0;
            }

            public void RefreshPresentation(WheelSpinResult result)
            {
                if (result.Icon != null) Icon = result.Icon;
            }

            public void AddAmount(int amount)
            {
                Amount += amount;
            }

            public RewardInventoryEntry ToEntry(string rewardId) => new RewardInventoryEntry(rewardId, DisplayName, Amount, Icon, AccentColor);
        }
    }
}
