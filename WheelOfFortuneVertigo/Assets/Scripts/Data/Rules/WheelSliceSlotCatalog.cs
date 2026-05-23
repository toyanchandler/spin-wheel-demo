using UnityEngine;

namespace Vertigo.Wheel.Data
{
    public static class WheelSliceSlotCatalog
    {
        private static readonly int[] RewardSlotsOffsetByBombMode = { 0, -1 };

        public static int RewardSlotsForZone(int sliceCount, WheelZoneGameplayProfile zoneGameplay)
        {
            int offset = RewardSlotsOffsetByBombMode[System.Convert.ToInt32(zoneGameplay.IncludesBombSlot)];
            return sliceCount + offset;
        }

        public static int BombIndexForZone(int sliceCount, WheelZoneGameplayProfile zoneGameplay)
        {
            int[] bombIndexByMode = { -1, Random.Range(0, sliceCount) };
            return bombIndexByMode[System.Convert.ToInt32(zoneGameplay.IncludesBombSlot)];
        }

        public static void ApplySlot(
            WheelSliceDefinition slice,
            bool isBombSlot,
            RewardDefinition rewardDefinition,
            Sprite bombIcon,
            WheelSliceSlotProfile rewardProfile,
            WheelSliceSlotProfile bombProfile)
        {
            WheelSliceSlotProfile[] profiles = { rewardProfile, bombProfile };
            Sprite[] icons = { rewardDefinition.WheelIcon, bombIcon };
            int profileIndex = System.Convert.ToInt32(isBombSlot);
            WheelSliceSlotProfile profile = profiles[profileIndex];

            slice.ApplySlot(
                isBombSlot,
                rewardDefinition,
                icons[profileIndex],
                profile.DisplayAmount,
                profile.DisplayColor,
                profile.Label,
                profile.ShowAmountLabel);
        }

        private static readonly bool[] ShowAmountByAmountFlag = { false, true };

        public static WheelSliceSlotProfile CreateRewardProfile(RewardDefinition reward)
        {
            return WheelSliceSlotProfile.CreateReward(
                reward.Label,
                reward.Amount,
                reward.AccentColor,
                ShowAmountByAmountFlag[System.Convert.ToInt32(reward.Amount > 1)]);
        }

        public static WheelSliceSlotProfile CreateBombProfile()
        {
            return WheelSliceSlotProfile.CreateBomb("Bomb", Color.white);
        }
    }
}
