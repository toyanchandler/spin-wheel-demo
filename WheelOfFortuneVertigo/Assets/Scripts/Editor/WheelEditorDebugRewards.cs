using System.Collections.Generic;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.EditorTools
{
    internal static class WheelEditorDebugRewards
    {
        public static int FillInventory(
            WheelGameState state,
            WheelGameSettings settings,
            int targetCount,
            string entryIdSuffix = "debug")
        {
            state.Inventory.Clear();

            var rewardPool = new List<RewardDefinition>();
            AddRewardsToPool(rewardPool, settings.GetRewardPool(ZoneType.Standard));
            AddRewardsToPool(rewardPool, settings.GetRewardPool(ZoneType.Safe));
            AddRewardsToPool(rewardPool, settings.GetRewardPool(ZoneType.Super));
            if (rewardPool.Count == 0)
            {
                Debug.LogWarning("No rewards are configured for debug loot commands.");
                return 0;
            }

            int added = 0;
            for (int i = 0; i < targetCount; i++)
            {
                RewardDefinition source = rewardPool[i % rewardPool.Count];
                if (source == null || string.IsNullOrEmpty(source.Id))
                {
                    continue;
                }

                RewardDefinition reward = RewardDefinition.Create(
                    source.Id + "_" + entryIdSuffix + "_" + i,
                    source.DisplayName,
                    source.Icon,
                    source.Amount,
                    source.Tier,
                    source.AccentColor,
                    source.WheelIcon);
                reward.CacheRuntimeText(settings.UiCopy.WinLabelFormat);

                var slice = new WheelSliceDefinition();
                slice.ApplySlot(
                    false,
                    reward,
                    reward.WheelIcon,
                    reward.Amount,
                    reward.AccentColor,
                    reward.Label,
                    true);

                state.Inventory.Add(new WheelSpinResult(added, slice), settings.UiCopy.InventoryFallbackRewardName);
                added++;
            }

            return added;
        }

        private static void AddRewardsToPool(List<RewardDefinition> target, IReadOnlyList<RewardDefinition> source)
        {
            if (source == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null)
                {
                    target.Add(source[i]);
                }
            }
        }
    }
}
