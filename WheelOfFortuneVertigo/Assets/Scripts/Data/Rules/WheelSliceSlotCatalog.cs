using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Data
{
    public static class WheelSliceSlotCatalog
    {
        public static int RewardSlotsForZone(int sliceCount, WheelZoneGameplayProfile zoneGameplay) => zoneGameplay.IncludesBombSlot ? sliceCount - 1 : sliceCount;

        public static int BombIndexForZone(int sliceCount, int zone, WheelZoneGameplayProfile zoneGameplay)
        {
            if (!zoneGameplay.IncludesBombSlot || sliceCount <= 0) return -1;
            int safeZone = Mathf.Max(1, zone);
            return (safeZone - 1) % sliceCount;
        }

        public static void ApplySlot(
            WheelSliceDefinition slice,
            bool isBombSlot,
            RewardDefinition rewardDefinition,
            Sprite bombIcon,
            WheelSliceSlotProfile rewardProfile,
            WheelSliceSlotProfile bombProfile)
        {
            WheelSliceSlotProfile profile = isBombSlot ? bombProfile : rewardProfile;
            Sprite icon = isBombSlot ? bombIcon : rewardDefinition.WheelIcon;

            slice.ApplySlot(
                isBombSlot,
                rewardDefinition,
                icon,
                profile.DisplayAmount,
                profile.DisplayColor,
                profile.Label,
                profile.ShowAmountLabel);
        }

        public static WheelSliceSlotProfile CreateRewardProfile(RewardDefinition reward)
        {
            return WheelSliceSlotProfile.CreateReward(
                reward.Label,
                reward.Amount,
                reward.AccentColor,
                reward.Amount > 1);
        }

        public static WheelSliceSlotProfile CreateBombProfile() => WheelSliceSlotProfile.CreateBomb("Bomb", Color.white);
    }
}
