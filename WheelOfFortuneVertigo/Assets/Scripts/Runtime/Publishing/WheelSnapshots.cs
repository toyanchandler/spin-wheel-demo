using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    public readonly struct WheelHudSnapshot
    {
        public readonly int Zone;
        public readonly WheelGamePhase Phase;
        public readonly Color BackgroundColor;
        public readonly WheelHudZoneLabelsSnapshot ZoneLabels;
        public readonly WheelHudMilestoneSnapshot Milestones;
        public readonly WheelHudStatusSnapshot StatusBar;
        public readonly WheelHudActionsSnapshot Actions;
        public readonly WheelHudExitConfirmationSnapshot ExitConfirmation;
        public readonly WheelHudRewardCardsSnapshot Rewards;

        public WheelHudSnapshot(
            int zone,
            WheelGamePhase phase,
            Color backgroundColor,
            WheelHudZoneLabelsSnapshot zoneLabels,
            WheelHudMilestoneSnapshot milestones,
            WheelHudStatusSnapshot statusBar,
            WheelHudActionsSnapshot actions,
            WheelHudExitConfirmationSnapshot exitConfirmation,
            WheelHudRewardCardsSnapshot rewards)
        {
            Zone = zone;
            Phase = phase;
            BackgroundColor = backgroundColor;
            ZoneLabels = zoneLabels;
            Milestones = milestones;
            StatusBar = statusBar;
            Actions = actions;
            ExitConfirmation = exitConfirmation;
            Rewards = rewards;
        }
    }

    public readonly struct WheelZoneSnapshot
    {
        public readonly WheelSlicePresentation[] SlicePresentations;
        public readonly int SliceCount;
        public readonly WheelSkinTier SkinTier;
        public readonly Sprite WheelBaseSprite;
        public readonly Sprite IndicatorSprite;
        public readonly Color BackgroundColor;

        public WheelZoneSnapshot(
            WheelSlicePresentation[] slicePresentations,
            int sliceCount,
            WheelSkinTier skinTier,
            Sprite wheelBaseSprite,
            Sprite indicatorSprite,
            Color backgroundColor)
        {
            SlicePresentations = slicePresentations;
            SliceCount = sliceCount;
            SkinTier = skinTier;
            WheelBaseSprite = wheelBaseSprite;
            IndicatorSprite = indicatorSprite;
            BackgroundColor = backgroundColor;
        }
    }

    public readonly struct WheelOutcomeSnapshot
    {
        public readonly WheelGamePhase Phase;
        public readonly string Title;
        public readonly string ResultText;
        public readonly Color ResultColor;
        public readonly Sprite Icon;
        public readonly Color IconImageColor;
        public readonly string RewardId;
        public readonly string RewardDisplayName;
        public readonly int RewardAmount;
        public readonly int SourceSliceIndex;
        public readonly bool HasPresentableOutcome;
        public readonly WheelOutcomePopupMotion Motion;

        public WheelOutcomeSnapshot(
            WheelGamePhase phase,
            string title,
            string resultText,
            Color resultColor,
            Sprite icon,
            Color iconImageColor,
            string rewardId,
            string rewardDisplayName,
            int rewardAmount,
            int sourceSliceIndex,
            bool hasPresentableOutcome,
            WheelOutcomePopupMotion motion)
        {
            Phase = phase;
            Title = title;
            ResultText = resultText;
            ResultColor = resultColor;
            Icon = icon;
            IconImageColor = iconImageColor;
            RewardId = rewardId;
            RewardDisplayName = rewardDisplayName;
            RewardAmount = rewardAmount;
            SourceSliceIndex = sourceSliceIndex;
            HasPresentableOutcome = hasPresentableOutcome;
            Motion = motion;
        }
    }

    public static class WheelSnapshotFactory
    {
        public static WheelHudSnapshot CreateHud(WheelGameState state) => WheelHudSnapshotBuilder.Create(state);

        public static WheelZoneSnapshot CreateZone(WheelGameState state) => WheelZoneSnapshotBuilder.Create(state);

        public static WheelOutcomeSnapshot CreateOutcome(
            WheelGamePhase phase,
            WheelSpinResult result,
            bool hasResult,
            RewardInventory inventory,
            WheelGameSettings settings)
        {
            return WheelOutcomeSnapshotBuilder.Create(phase, result, hasResult, inventory, settings);
        }
    }
}
