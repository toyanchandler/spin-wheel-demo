using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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

        public int Count { get { return _rewards.Count; } }

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
            RewardStack stack = StackAcquireActions[Convert.ToInt32(alreadyTracked)](this, result, fallbackRewardName);
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
                if (index >= buffer.Length)
                {
                    break;
                }

                string rewardId = _rewardOrder[i];
                RewardStack stack;
                if (!_rewards.TryGetValue(rewardId, out stack))
                {
                    continue;
                }

                buffer[index] = stack.ToEntry(rewardId);
                index++;
            }

            return index;
        }

        public string BuildSummary(string emptySummary)
        {
            return SummaryActions[Convert.ToInt32(_dirty)](this, emptySummary);
        }

        private RewardStack CreateStack(WheelSpinResult result, string fallbackRewardName)
        {
            RewardStack stack = new RewardStack(result, fallbackRewardName);
            _rewards.Add(result.RewardId, stack);
            return stack;
        }

        private RewardStack GetStack(WheelSpinResult result, string fallbackRewardName)
        {
            return _rewards[result.RewardId];
        }

        private void MoveRewardToNewest(string rewardId, bool alreadyTracked)
        {
            if (alreadyTracked)
            {
                _rewardOrder.Remove(rewardId);
            }

            _rewardOrder.Add(rewardId);
        }

        private string ReadCachedSummary(string emptySummary)
        {
            return string.IsNullOrEmpty(_summary) ? emptySummary : _summary;
        }

        private string RebuildSummary(string emptySummary)
        {
            _builder.Clear();
            int index = 0;
            for (int i = 0; i < _rewardOrder.Count; i++)
            {
                string rewardId = _rewardOrder[i];
                RewardStack stack;
                if (!_rewards.TryGetValue(rewardId, out stack))
                {
                    continue;
                }

                SeparatorActions[Convert.ToInt32(index > 0)](_builder);
                _builder.Append(stack.DisplayName);
                _builder.Append(" x");
                _builder.Append(stack.Amount);
                index++;
            }

            _summary = _builder.Length == 0 ? emptySummary : _builder.ToString();
            _dirty = false;
            return _summary;
        }

        private static readonly Func<RewardInventory, WheelSpinResult, string, RewardStack>[] StackAcquireActions =
        {
            (inventory, result, fallbackRewardName) => inventory.CreateStack(result, fallbackRewardName),
            (inventory, result, fallbackRewardName) => inventory.GetStack(result, fallbackRewardName)
        };

        private static readonly Func<RewardInventory, string, string>[] SummaryActions =
        {
            (inventory, emptySummary) => inventory.ReadCachedSummary(emptySummary),
            (inventory, emptySummary) => inventory.RebuildSummary(emptySummary)
        };

        private static readonly Action<StringBuilder>[] SeparatorActions =
        {
            builder => { },
            builder => builder.Append("  ")
        };

        private sealed class RewardStack
        {
            private static readonly Func<Sprite, Sprite, Sprite>[] IconRefreshActions =
            {
                (current, incoming) => current,
                (current, incoming) => incoming
            };

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
                Icon = IconRefreshActions[Convert.ToInt32(result.Icon != null)](Icon, result.Icon);
            }

            public void AddAmount(int amount)
            {
                Amount += amount;
            }

            public RewardInventoryEntry ToEntry(string rewardId)
            {
                return new RewardInventoryEntry(rewardId, DisplayName, Amount, Icon, AccentColor);
            }
        }
    }
}
