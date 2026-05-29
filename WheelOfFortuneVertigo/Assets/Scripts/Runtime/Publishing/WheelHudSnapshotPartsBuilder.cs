using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>One builder method per HUD snapshot section — keeps <see cref="WheelHudSnapshotBuilder"/> thin.</summary>
    public static class WheelHudSnapshotPartsBuilder
    {
        public static WheelHudZoneLabelsSnapshot BuildZoneLabels(
            WheelGameState state,
            WheelUiCopyCatalog catalog,
            WheelThemeSettings theme)
        {
            WheelZoneUiCopy zoneCopy = catalog.GetZoneCopy(state.ZoneType);
            return new WheelHudZoneLabelsSnapshot(
                zoneCopy.Label,
                theme.PrimaryTextColor,
                catalog.ResolveColor(zoneCopy.LabelColorKey, theme));
        }

        public static WheelHudMilestoneSnapshot BuildMilestones(
            WheelGameState state,
            WheelGameSettings settings,
            WheelUiCopyCatalog catalog,
            WheelThemeSettings theme)
        {
            int nextSafeZone = ResolveNextZone(state.Zone, settings.SafeZoneInterval);
            int nextSuperZone = ResolveNextZone(state.Zone, settings.SuperZoneInterval);
            return new WheelHudMilestoneSnapshot(
                settings.SafeZoneInterval,
                settings.SuperZoneInterval,
                Mathf.Max(1, settings.SafeZoneInterval),
                Mathf.Max(1, settings.SuperZoneInterval),
                nextSafeZone,
                nextSuperZone,
                string.Format(catalog.SafeMilestoneBadgeFormat, nextSafeZone),
                string.Format(catalog.SuperMilestoneBadgeFormat, nextSuperZone),
                theme.SafeMilestoneBadgeBackground,
                theme.SuperMilestoneBadgeBackground);
        }

        public static WheelHudStatusSnapshot BuildStatus(
            WheelGameState state,
            WheelUiCopyCatalog catalog,
            WheelThemeSettings theme)
        {
            string winLabel = state.HasLastResult ? state.LastResult.WinLabel : null;
            return new WheelHudStatusSnapshot(
                catalog.ResolveStatusMessage(state.Phase, state.ZoneType, winLabel),
                !catalog.ShouldHideStatusBar(state.Phase),
                theme.SecondaryTextColor);
        }

        public static WheelHudActionsSnapshot BuildActions(WheelGameState state, WheelUiCopyCatalog catalog)
        {
            return new WheelHudActionsSnapshot(
                state.CanSpin,
                state.CanLeave,
                state.CanRestart,
                state.Phase == WheelGamePhase.Bombed,
                state.Phase == WheelGamePhase.CashedOut,
                !catalog.ShouldHideOutcomePopup(state.Phase),
                catalog.SpinButtonLabel,
                catalog.LeaveButtonLabel,
                catalog.RestartButtonLabel);
        }

        public static WheelHudExitConfirmationSnapshot BuildExitConfirmation(WheelUiCopyCatalog catalog)
        {
            return new WheelHudExitConfirmationSnapshot(
                catalog.ExitConfirmationTitle,
                catalog.ExitConfirmationBody,
                catalog.ExitCollectButtonLabel,
                catalog.ExitComeBackButtonLabel);
        }

        public static WheelHudRewardCardsSnapshot BuildRewardCards(
            WheelGameState state,
            WheelUiCopyCatalog catalog,
            WheelLayoutSettings layout)
        {
            RewardInventoryEntry[] rewardCards = CopyRewardCards(state.Inventory, out int rewardCardCount);
            return new WheelHudRewardCardsSnapshot(
                catalog.RewardOpeningTitle,
                catalog.DefaultRewardTitle,
                layout.RewardCardFrameSprite,
                rewardCardCount,
                rewardCards);
        }

        private static RewardInventoryEntry[] CopyRewardCards(RewardInventory inventory, out int rewardCardCount)
        {
            int bufferSize = inventory.Count;
            if (bufferSize <= 0)
            {
                rewardCardCount = 0;
                return System.Array.Empty<RewardInventoryEntry>();
            }

            var buffer = new RewardInventoryEntry[bufferSize];
            rewardCardCount = inventory.CopyEntries(buffer);
            if (rewardCardCount == buffer.Length)
            {
                return buffer;
            }

            var rewardCards = new RewardInventoryEntry[rewardCardCount];
            System.Array.Copy(buffer, rewardCards, rewardCardCount);
            return rewardCards;
        }

        private static int ResolveNextZone(int currentZone, int interval)
        {
            int safeInterval = Mathf.Max(1, interval);
            int remainder = currentZone % safeInterval;
            if (remainder == 0)
            {
                return currentZone;
            }

            return currentZone + (safeInterval - remainder);
        }
    }
}
