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

        public string ZoneTypeLabel { get { return ZoneLabels.ZoneTypeLabel; } }
        public Color ZoneNumberColor { get { return ZoneLabels.ZoneNumberColor; } }
        public Color ZoneTypeColor { get { return ZoneLabels.ZoneTypeColor; } }

        public int SafeZoneInterval { get { return Milestones.SafeZoneInterval; } }
        public int SuperZoneInterval { get { return Milestones.SuperZoneInterval; } }
        public int NextSafeZone { get { return Milestones.NextSafeZone; } }
        public int NextSuperZone { get { return Milestones.NextSuperZone; } }
        public string SafeMilestoneBadgeText { get { return Milestones.SafeMilestoneBadgeText; } }
        public string SuperMilestoneBadgeText { get { return Milestones.SuperMilestoneBadgeText; } }
        public Color SafeMilestoneBadgeColor { get { return Milestones.SafeMilestoneBadgeColor; } }
        public Color SuperMilestoneBadgeColor { get { return Milestones.SuperMilestoneBadgeColor; } }

        public string StatusText { get { return StatusBar.StatusText; } }
        public bool IsStatusVisible { get { return StatusBar.IsStatusVisible; } }
        public Color StatusColor { get { return StatusBar.StatusColor; } }

        public bool CanSpin { get { return Actions.CanSpin; } }
        public bool CanLeave { get { return Actions.CanLeave; } }
        public bool CanRestart { get { return Actions.CanRestart; } }
        public bool IsOutcomePopupAllowed { get { return Actions.IsOutcomePopupAllowed; } }
        public string SpinButtonLabel { get { return Actions.SpinButtonLabel; } }
        public string LeaveButtonLabel { get { return Actions.LeaveButtonLabel; } }
        public string RestartButtonLabel { get { return Actions.RestartButtonLabel; } }

        public string ExitConfirmationTitle { get { return ExitConfirmation.Title; } }
        public string ExitConfirmationBody { get { return ExitConfirmation.Body; } }
        public string ExitCollectButtonLabel { get { return ExitConfirmation.CollectButtonLabel; } }
        public string ExitComeBackButtonLabel { get { return ExitConfirmation.ComeBackButtonLabel; } }

        public string RewardOpeningTitle { get { return Rewards.RewardOpeningTitle; } }
        public string DefaultRewardTitle { get { return Rewards.DefaultRewardTitle; } }
        public Sprite RewardCardFrameSprite { get { return Rewards.RewardCardFrameSprite; } }
        public int RewardCardCount { get { return Rewards.RewardCardCount; } }
        public RewardInventoryEntry[] RewardCards { get { return Rewards.RewardCards; } }
    }

    public readonly struct WheelZoneSnapshot
    {
        public readonly WheelSliceDefinition[] Slices;
        public readonly int SliceCount;
        public readonly WheelSkinTier SkinTier;
        public readonly Color BackgroundColor;

        public WheelZoneSnapshot(
            WheelSliceDefinition[] slices,
            int sliceCount,
            WheelSkinTier skinTier,
            Color backgroundColor)
        {
            Slices = slices;
            SliceCount = sliceCount;
            SkinTier = skinTier;
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
        public static WheelHudSnapshot CreateHud(WheelGameState state)
        {
            WheelGameSettings settings = state.Settings;
            WheelUiCopyCatalog catalog = settings.UiCopy;
            WheelThemeSettings theme = settings.Theme;
            WheelLayoutSettings layout = settings.Layout;
            ZoneType zoneType = state.ZoneType;
            WheelZoneUiCopy zoneCopy = catalog.GetZoneCopy(zoneType);
            string winLabel = state.HasLastResult ? state.LastResult.WinLabel : null;
            RewardInventoryEntry[] rewardCards = CopyRewardCards(state.Inventory, out int rewardCardCount);
            int nextSafeZone = ResolveNextZone(state.Zone, settings.SafeZoneInterval);
            int nextSuperZone = ResolveNextZone(state.Zone, settings.SuperZoneInterval);

            return new WheelHudSnapshot(
                state.Zone,
                state.Phase,
                theme.BackgroundColor,
                new WheelHudZoneLabelsSnapshot(
                    zoneCopy.Label,
                    theme.PrimaryTextColor,
                    catalog.ResolveColor(zoneCopy.LabelColorKey, theme)),
                new WheelHudMilestoneSnapshot(
                    settings.SafeZoneInterval,
                    settings.SuperZoneInterval,
                    nextSafeZone,
                    nextSuperZone,
                    string.Format(catalog.SafeMilestoneBadgeFormat, nextSafeZone),
                    string.Format(catalog.SuperMilestoneBadgeFormat, nextSuperZone),
                    theme.SafeMilestoneBadgeBackground,
                    theme.SuperMilestoneBadgeBackground),
                new WheelHudStatusSnapshot(
                    catalog.ResolveStatusMessage(state.Phase, zoneType, winLabel),
                    !catalog.ShouldHideStatusBar(state.Phase),
                    theme.SecondaryTextColor),
                new WheelHudActionsSnapshot(
                    state.CanSpin,
                    state.CanLeave,
                    state.CanRestart,
                    !catalog.ShouldHideOutcomePopup(state.Phase),
                    catalog.SpinButtonLabel,
                    catalog.LeaveButtonLabel,
                    catalog.RestartButtonLabel),
                new WheelHudExitConfirmationSnapshot(
                    catalog.ExitConfirmationTitle,
                    catalog.ExitConfirmationBody,
                    catalog.ExitCollectButtonLabel,
                    catalog.ExitComeBackButtonLabel),
                new WheelHudRewardCardsSnapshot(
                    catalog.RewardOpeningTitle,
                    catalog.DefaultRewardTitle,
                    layout.RewardCardFrameSprite,
                    rewardCardCount,
                    rewardCards));
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

        public static WheelZoneSnapshot CreateZone(WheelGameState state)
        {
            WheelGameSettings settings = state.Settings;
            WheelZoneUiCopy zoneCopy = settings.UiCopy.GetZoneCopy(state.ZoneType);

            return new WheelZoneSnapshot(
                state.Slices,
                state.SliceCount,
                zoneCopy.SkinTier,
                settings.Theme.BackgroundColor);
        }

        public static WheelOutcomeSnapshot CreateOutcome(
            WheelGamePhase phase,
            WheelSpinResult result,
            bool hasResult,
            RewardInventory inventory,
            WheelGameSettings settings)
        {
            WheelOutcomeUiCopy outcomeCopy = settings.UiCopy.GetOutcomeCopy(phase);
            string resultText = ResolveOutcomeResultText(
                outcomeCopy.ResultSource,
                outcomeCopy.ResultFallback,
                result,
                hasResult,
                inventory,
                settings.UiCopy);
            string title = outcomeCopy.Title;
            ApplyRewardOutcomeResultText(phase, result, hasResult, ref resultText);
            bool showIcon = outcomeCopy.ShowIcon && hasResult && result.Icon != null;
            bool hasPresentableOutcome = hasResult && showIcon;

            return new WheelOutcomeSnapshot(
                phase,
                title,
                resultText,
                settings.UiCopy.ResolveColor(outcomeCopy.ResultColorKey, settings.Theme),
                showIcon ? result.Icon : null,
                showIcon ? ResolveOutcomeIconColor(result, hasResult) : Color.clear,
                hasResult ? result.RewardId : string.Empty,
                hasResult ? result.DisplayName : string.Empty,
                hasResult ? result.Amount : 0,
                hasResult ? result.SliceIndex : -1,
                hasPresentableOutcome,
                settings.OutcomePopupMotionCatalog.Motion);
        }

        private static Color ResolveOutcomeIconColor(WheelSpinResult result, bool hasResult)
        {
            if (!hasResult)
            {
                return Color.clear;
            }

            Color color = result.AccentColor;
            color.a = 1f;
            if (color.maxColorComponent <= 0.02f)
            {
                return Color.white;
            }

            return color;
        }

        private static void ApplyRewardOutcomeResultText(
            WheelGamePhase phase,
            WheelSpinResult result,
            bool hasResult,
            ref string resultText)
        {
            if (phase != WheelGamePhase.Won || !hasResult || result.IsBomb)
            {
                return;
            }

            string displayName = string.IsNullOrEmpty(result.DisplayName) ? resultText : result.DisplayName;
            resultText = result.Amount > 1 ? string.Format("{0} x{1}", displayName, result.Amount) : displayName;
        }

        private static string ResolveOutcomeResultText(
            WheelOutcomeResultSource resultSource,
            string resultFallback,
            WheelSpinResult result,
            bool hasResult,
            RewardInventory inventory,
            WheelUiCopyCatalog catalog)
        {
            switch (resultSource)
            {
                case WheelOutcomeResultSource.SpinResultLabel:
                    return hasResult ? result.WinLabel : resultFallback;
                case WheelOutcomeResultSource.InventorySummary:
                    return inventory.BuildSummary(catalog.InventoryEmptySummary);
                case WheelOutcomeResultSource.StaticFallback:
                default:
                    return resultFallback;
            }
        }
    }
}
