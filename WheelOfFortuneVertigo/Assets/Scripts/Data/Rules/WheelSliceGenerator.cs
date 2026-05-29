using System;
using System.Collections.Generic;

namespace Vertigo.Wheel.Data
{
    public static class WheelSliceGenerator
    {
        public static int FillSlicesForZone(WheelGameSettings settings, int zone, WheelSliceDefinition[] buffer)
        {
            ZoneType zoneType = settings.GetZoneType(zone);
            WheelZoneGameplayProfile zoneGameplay = settings.GetZoneGameplay(zoneType);
            IReadOnlyList<RewardDefinition> source = settings.GetRewardPool(zoneType);
            if (source == null || source.Count == 0)
            {
                throw new InvalidOperationException("WheelGameSettings requires at least one reward for " + zoneType + " zones.");
            }

            int sliceCount = settings.SliceCount;
            int bombIndex = WheelSliceSlotCatalog.BombIndexForZone(sliceCount, zone, zoneGameplay);
            WheelSliceSlotProfile bombProfile = WheelSliceSlotCatalog.CreateBombProfile();
            int rewardIndex = 0;

            for (int i = 0; i < sliceCount; i++)
            {
                bool isBombSlot = i == bombIndex;
                RewardDefinition poolReward = source[(zone + rewardIndex) % source.Count];
                ApplySliceSlot(
                    buffer[i],
                    isBombSlot,
                    isBombSlot ? settings.BombReward : poolReward,
                    poolReward,
                    settings.BombReward.WheelIcon,
                    bombProfile);

                if (!isBombSlot)
                {
                    rewardIndex++;
                }
            }

            return sliceCount;
        }

        private static void ApplySliceSlot(
            WheelSliceDefinition target,
            bool isBombSlot,
            RewardDefinition reward,
            RewardDefinition poolReward,
            UnityEngine.Sprite bombWheelIcon,
            WheelSliceSlotProfile bombProfile)
        {
            WheelSliceSlotProfile rewardProfile = WheelSliceSlotCatalog.CreateRewardProfile(poolReward);
            WheelSliceSlotCatalog.ApplySlot(
                target,
                isBombSlot,
                reward,
                bombWheelIcon,
                rewardProfile,
                bombProfile);
        }
    }
}
