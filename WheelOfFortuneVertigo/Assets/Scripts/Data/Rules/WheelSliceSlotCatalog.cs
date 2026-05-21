using UnityEngine;

namespace Vertigo.Wheel.Data
{
    public static class WheelSliceSlotCatalog
    {
        private static readonly int[] RewardSlotsOffsetByBombMode = { 0, -1 };

        public static int RewardSlotsForZone(int sliceCount, WheelZoneGameplayProfile zoneGameplay)
        {
            int offset = RewardSlotsOffsetByBombMode[System.Convert.ToInt32(zoneGameplay.includesBombSlot)];
            return sliceCount + offset;
        }

        public static int BombIndexForZone(int sliceCount, WheelZoneGameplayProfile zoneGameplay)
        {
            int[] bombIndexByMode = { -1, Random.Range(0, sliceCount) };
            return bombIndexByMode[System.Convert.ToInt32(zoneGameplay.includesBombSlot)];
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
            Sprite[] icons = { rewardDefinition.icon, bombIcon };
            int profileIndex = System.Convert.ToInt32(isBombSlot);

            slice.isBomb = isBombSlot;
            slice.reward = rewardDefinition;
            slice.displayIcon = icons[profileIndex];
            slice.displayAmount = profiles[profileIndex].displayAmount;
            slice.displayColor = profiles[profileIndex].displayColor;
            slice.displayLabel = profiles[profileIndex].label;
            slice.showAmountLabel = profiles[profileIndex].showAmountLabel;
        }

        private static readonly bool[] ShowAmountByAmountFlag = { false, true };

        public static WheelSliceSlotProfile CreateRewardProfile(RewardDefinition reward)
        {
            return new WheelSliceSlotProfile
            {
                label = reward.Label,
                displayAmount = reward.amount,
                displayColor = reward.accentColor,
                showAmountLabel = ShowAmountByAmountFlag[System.Convert.ToInt32(reward.amount > 1)]
            };
        }

        public static WheelSliceSlotProfile CreateBombProfile()
        {
            return new WheelSliceSlotProfile
            {
                label = "Bomb",
                displayColor = Color.white
            };
        }
    }
}
