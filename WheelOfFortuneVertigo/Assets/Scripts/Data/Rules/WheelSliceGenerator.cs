using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    public static class WheelSliceGenerator
    {
        public static int FillSlicesForZone(WheelGameSettings settings, int zone, WheelSliceDefinition[] buffer)
        {
            ZoneType zoneType = settings.GetZoneType(zone);
            WheelZoneGameplayProfile zoneGameplay = settings.GetZoneGameplay(zoneType);
            List<RewardDefinition> source = settings.GetRewardPool(zoneType);
            int sliceCount = settings.SliceCount;
            int bombIndex = WheelSliceSlotCatalog.BombIndexForZone(sliceCount, zoneGameplay);
            WheelSliceSlotProfile bombProfile = WheelSliceSlotCatalog.CreateBombProfile();

            int rewardIndex = 0;
            for (int i = 0; i < sliceCount; i++)
            {
                int slotKind = System.Convert.ToInt32(i == bombIndex);
                RewardDefinition poolReward = source[(zone + rewardIndex) % source.Count];
                RewardDefinition[] rewardsByKind = { poolReward, settings.BombReward };
                WheelSliceSlotProfile rewardProfile = WheelSliceSlotCatalog.CreateRewardProfile(poolReward);
                WheelSliceSlotProfile[] profilesByKind = { rewardProfile, bombProfile };

                WheelSliceSlotCatalog.ApplySlot(
                    buffer[i],
                    slotKind == 1,
                    rewardsByKind[slotKind],
                    settings.BombReward.Icon,
                    profilesByKind[0],
                    bombProfile);

                rewardIndex += 1 - slotKind;
            }

            return sliceCount;
        }
    }
}
