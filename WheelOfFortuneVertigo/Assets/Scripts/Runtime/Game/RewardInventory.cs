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
        private const string EmptySummary = "No rewards";

        private readonly Dictionary<string, RewardStack> _rewards = new Dictionary<string, RewardStack>();
        private readonly StringBuilder _builder = new StringBuilder(128);
        private string _summary = EmptySummary;
        private bool _dirty;

        public int Count { get { return _rewards.Count; } }

        public void Clear()
        {
            _rewards.Clear();
            _summary = EmptySummary;
            _dirty = false;
        }

        public void Add(WheelSpinResult result)
        {
            RewardStack stack = StackAcquireActions[Convert.ToInt32(_rewards.ContainsKey(result.RewardId))](this, result);
            stack.RefreshPresentation(result);
            stack.Amount += result.Amount;
            _dirty = true;
        }

        public int CopyEntries(RewardInventoryEntry[] buffer)
        {
            int index = 0;
            foreach (KeyValuePair<string, RewardStack> pair in _rewards)
            {
                if (index >= buffer.Length)
                {
                    break;
                }

                buffer[index] = pair.Value.ToEntry(pair.Key);
                index++;
            }

            return index;
        }

        public string BuildSummary()
        {
            return SummaryActions[Convert.ToInt32(_dirty)](this);
        }

        private RewardStack CreateStack(WheelSpinResult result)
        {
            RewardStack stack = new RewardStack(result);
            _rewards.Add(result.RewardId, stack);
            return stack;
        }

        private RewardStack GetStack(WheelSpinResult result)
        {
            return _rewards[result.RewardId];
        }

        private string ReadCachedSummary()
        {
            return _summary;
        }

        private string RebuildSummary()
        {
            _builder.Clear();
            int index = 0;
            foreach (KeyValuePair<string, RewardStack> pair in _rewards)
            {
                SeparatorActions[Convert.ToInt32(index > 0)](_builder);
                _builder.Append(pair.Value.DisplayName);
                _builder.Append(" x");
                _builder.Append(pair.Value.Amount);
                index++;
            }

            _summary = _builder.Length == 0 ? EmptySummary : _builder.ToString();
            _dirty = false;
            return _summary;
        }

        private static readonly Func<RewardInventory, WheelSpinResult, RewardStack>[] StackAcquireActions =
        {
            (inventory, result) => inventory.CreateStack(result),
            (inventory, result) => inventory.GetStack(result)
        };

        private static readonly Func<RewardInventory, string>[] SummaryActions =
        {
            inventory => inventory.ReadCachedSummary(),
            inventory => inventory.RebuildSummary()
        };

        private static readonly Action<StringBuilder>[] SeparatorActions =
        {
            builder => { },
            builder => builder.Append("  ")
        };

        private sealed class RewardStack
        {
            private static readonly Func<string, string>[] DisplayNameResolve =
            {
                name => name,
                name => "Reward"
            };

            private static readonly Func<Sprite, Sprite, Sprite>[] IconRefreshActions =
            {
                (current, incoming) => current,
                (current, incoming) => incoming
            };

            public readonly string DisplayName;
            public Sprite Icon { get; private set; }
            public readonly Color AccentColor;
            public int Amount;

            public RewardStack(WheelSpinResult result)
            {
                DisplayName = DisplayNameResolve[Convert.ToInt32(string.IsNullOrEmpty(result.DisplayName))](result.DisplayName);
                Icon = result.Icon;
                AccentColor = result.AccentColor;
                Amount = 0;
            }

            public void RefreshPresentation(WheelSpinResult result)
            {
                Icon = IconRefreshActions[Convert.ToInt32(result.Icon != null)](Icon, result.Icon);
            }

            public RewardInventoryEntry ToEntry(string rewardId)
            {
                return new RewardInventoryEntry(rewardId, DisplayName, Amount, Icon, AccentColor);
            }
        }
    }
}
