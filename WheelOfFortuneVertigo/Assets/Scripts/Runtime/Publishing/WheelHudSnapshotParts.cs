using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    public readonly struct WheelHudZoneLabelsSnapshot
    {
        public readonly string ZoneTypeLabel;
        public readonly Color ZoneNumberColor;
        public readonly Color ZoneTypeColor;

        public WheelHudZoneLabelsSnapshot(string zoneTypeLabel, Color zoneNumberColor, Color zoneTypeColor)
        {
            ZoneTypeLabel = zoneTypeLabel;
            ZoneNumberColor = zoneNumberColor;
            ZoneTypeColor = zoneTypeColor;
        }
    }

    public readonly struct WheelHudMilestoneSnapshot
    {
        public readonly int SafeZoneInterval;
        public readonly int SuperZoneInterval;
        public readonly int DisplaySafeZoneInterval;
        public readonly int DisplaySuperZoneInterval;
        public readonly int NextSafeZone;
        public readonly int NextSuperZone;
        public readonly string SafeMilestoneBadgeText;
        public readonly string SuperMilestoneBadgeText;
        public readonly Color SafeMilestoneBadgeColor;
        public readonly Color SuperMilestoneBadgeColor;

        public WheelHudMilestoneSnapshot(
            int safeZoneInterval,
            int superZoneInterval,
            int displaySafeZoneInterval,
            int displaySuperZoneInterval,
            int nextSafeZone,
            int nextSuperZone,
            string safeMilestoneBadgeText,
            string superMilestoneBadgeText,
            Color safeMilestoneBadgeColor,
            Color superMilestoneBadgeColor)
        {
            SafeZoneInterval = safeZoneInterval;
            SuperZoneInterval = superZoneInterval;
            DisplaySafeZoneInterval = displaySafeZoneInterval;
            DisplaySuperZoneInterval = displaySuperZoneInterval;
            NextSafeZone = nextSafeZone;
            NextSuperZone = nextSuperZone;
            SafeMilestoneBadgeText = safeMilestoneBadgeText;
            SuperMilestoneBadgeText = superMilestoneBadgeText;
            SafeMilestoneBadgeColor = safeMilestoneBadgeColor;
            SuperMilestoneBadgeColor = superMilestoneBadgeColor;
        }
    }

    public readonly struct WheelHudStatusSnapshot
    {
        public readonly string StatusText;
        public readonly bool IsStatusVisible;
        public readonly Color StatusColor;

        public WheelHudStatusSnapshot(string statusText, bool isStatusVisible, Color statusColor)
        {
            StatusText = statusText;
            IsStatusVisible = isStatusVisible;
            StatusColor = statusColor;
        }
    }

    public readonly struct WheelHudActionsSnapshot
    {
        public readonly bool CanSpin;
        public readonly bool CanLeave;
        public readonly bool CanRestart;
        public readonly bool ShowOutcomeRetryRestart;
        public readonly bool ShowRewardOpeningRestart;
        public readonly bool IsOutcomePopupAllowed;
        public readonly string SpinButtonLabel;
        public readonly string LeaveButtonLabel;
        public readonly string RestartButtonLabel;

        public WheelHudActionsSnapshot(
            bool canSpin,
            bool canLeave,
            bool canRestart,
            bool showOutcomeRetryRestart,
            bool showRewardOpeningRestart,
            bool isOutcomePopupAllowed,
            string spinButtonLabel,
            string leaveButtonLabel,
            string restartButtonLabel)
        {
            CanSpin = canSpin;
            CanLeave = canLeave;
            CanRestart = canRestart;
            ShowOutcomeRetryRestart = showOutcomeRetryRestart;
            ShowRewardOpeningRestart = showRewardOpeningRestart;
            IsOutcomePopupAllowed = isOutcomePopupAllowed;
            SpinButtonLabel = spinButtonLabel;
            LeaveButtonLabel = leaveButtonLabel;
            RestartButtonLabel = restartButtonLabel;
        }
    }

    public readonly struct WheelHudExitConfirmationSnapshot
    {
        public readonly string Title;
        public readonly string Body;
        public readonly string CollectButtonLabel;
        public readonly string ComeBackButtonLabel;

        public WheelHudExitConfirmationSnapshot(
            string title,
            string body,
            string collectButtonLabel,
            string comeBackButtonLabel)
        {
            Title = title;
            Body = body;
            CollectButtonLabel = collectButtonLabel;
            ComeBackButtonLabel = comeBackButtonLabel;
        }
    }

    public readonly struct WheelHudRewardCardsSnapshot
    {
        public readonly string RewardOpeningTitle;
        public readonly string DefaultRewardTitle;
        public readonly Sprite RewardCardFrameSprite;
        public readonly int RewardCardCount;
        public readonly RewardInventoryEntry[] RewardCards;

        public WheelHudRewardCardsSnapshot(
            string rewardOpeningTitle,
            string defaultRewardTitle,
            Sprite rewardCardFrameSprite,
            int rewardCardCount,
            RewardInventoryEntry[] rewardCards)
        {
            RewardOpeningTitle = rewardOpeningTitle;
            DefaultRewardTitle = defaultRewardTitle;
            RewardCardFrameSprite = rewardCardFrameSprite;
            RewardCardCount = rewardCardCount;
            RewardCards = rewardCards;
        }
    }
}
