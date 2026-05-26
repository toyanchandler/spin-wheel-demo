using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    public readonly struct WheelHudSnapshot
    {
        public readonly int Zone;
        public readonly WheelGamePhase Phase;
        public readonly string ZoneTypeLabel;
        public readonly Color ZoneNumberColor;
        public readonly Color ZoneTypeColor;
        public readonly int SafeZoneInterval;
        public readonly int SuperZoneInterval;
        public readonly int NextSafeZone;
        public readonly int NextSuperZone;
        public readonly string SafeMilestoneBadgeText;
        public readonly string SuperMilestoneBadgeText;
        public readonly Color SafeMilestoneBadgeColor;
        public readonly Color SuperMilestoneBadgeColor;
        public readonly string StatusText;
        public readonly bool IsStatusVisible;
        public readonly Color StatusColor;
        public readonly Color BackgroundColor;
        public readonly bool CanSpin;
        public readonly bool CanLeave;
        public readonly bool CanRestart;
        public readonly bool IsOutcomePopupAllowed;
        public readonly string SpinButtonLabel;
        public readonly string LeaveButtonLabel;
        public readonly string RestartButtonLabel;
        public readonly string RewardOpeningTitle;
        public readonly string DefaultRewardTitle;
        public readonly string ExitConfirmationTitle;
        public readonly string ExitConfirmationBody;
        public readonly string ExitCollectButtonLabel;
        public readonly string ExitComeBackButtonLabel;
        public readonly Sprite RewardCardFrameSprite;
        public readonly int RewardCardCount;
        public readonly RewardInventoryEntry[] RewardCards;

        public WheelHudSnapshot(
            int zone,
            WheelGamePhase phase,
            string zoneTypeLabel,
            Color zoneNumberColor,
            Color zoneTypeColor,
            int safeZoneInterval,
            int superZoneInterval,
            int nextSafeZone,
            int nextSuperZone,
            string safeMilestoneBadgeText,
            string superMilestoneBadgeText,
            Color safeMilestoneBadgeColor,
            Color superMilestoneBadgeColor,
            string statusText,
            bool isStatusVisible,
            Color statusColor,
            Color backgroundColor,
            bool canSpin,
            bool canLeave,
            bool canRestart,
            bool isOutcomePopupAllowed,
            string spinButtonLabel,
            string leaveButtonLabel,
            string restartButtonLabel,
            string rewardOpeningTitle,
            string defaultRewardTitle,
            string exitConfirmationTitle,
            string exitConfirmationBody,
            string exitCollectButtonLabel,
            string exitComeBackButtonLabel,
            Sprite rewardCardFrameSprite,
            int rewardCardCount,
            RewardInventoryEntry[] rewardCards)
        {
            Zone = zone;
            Phase = phase;
            ZoneTypeLabel = zoneTypeLabel;
            ZoneNumberColor = zoneNumberColor;
            ZoneTypeColor = zoneTypeColor;
            SafeZoneInterval = safeZoneInterval;
            SuperZoneInterval = superZoneInterval;
            NextSafeZone = nextSafeZone;
            NextSuperZone = nextSuperZone;
            SafeMilestoneBadgeText = safeMilestoneBadgeText;
            SuperMilestoneBadgeText = superMilestoneBadgeText;
            SafeMilestoneBadgeColor = safeMilestoneBadgeColor;
            SuperMilestoneBadgeColor = superMilestoneBadgeColor;
            StatusText = statusText;
            IsStatusVisible = isStatusVisible;
            StatusColor = statusColor;
            BackgroundColor = backgroundColor;
            CanSpin = canSpin;
            CanLeave = canLeave;
            CanRestart = canRestart;
            IsOutcomePopupAllowed = isOutcomePopupAllowed;
            SpinButtonLabel = spinButtonLabel;
            LeaveButtonLabel = leaveButtonLabel;
            RestartButtonLabel = restartButtonLabel;
            RewardOpeningTitle = rewardOpeningTitle;
            DefaultRewardTitle = defaultRewardTitle;
            ExitConfirmationTitle = exitConfirmationTitle;
            ExitConfirmationBody = exitConfirmationBody;
            ExitCollectButtonLabel = exitCollectButtonLabel;
            ExitComeBackButtonLabel = exitComeBackButtonLabel;
            RewardCardFrameSprite = rewardCardFrameSprite;
            RewardCardCount = rewardCardCount;
            RewardCards = rewardCards;
        }
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
                zoneCopy.Label,
                theme.PrimaryTextColor,
                catalog.ResolveColor(zoneCopy.LabelColorKey, theme),
                settings.SafeZoneInterval,
                settings.SuperZoneInterval,
                nextSafeZone,
                nextSuperZone,
                string.Format(catalog.SafeMilestoneBadgeFormat, nextSafeZone),
                string.Format(catalog.SuperMilestoneBadgeFormat, nextSuperZone),
                theme.SafeMilestoneBadgeBackground,
                theme.SuperMilestoneBadgeBackground,
                catalog.ResolveStatusMessage(state.Phase, zoneType, winLabel),
                !catalog.ShouldHideStatusBar(state.Phase),
                theme.SecondaryTextColor,
                theme.BackgroundColor,
                state.CanSpin,
                state.CanLeave,
                state.CanRestart,
                !catalog.ShouldHideOutcomePopup(state.Phase),
                catalog.SpinButtonLabel,
                catalog.LeaveButtonLabel,
                catalog.RestartButtonLabel,
                catalog.RewardOpeningTitle,
                catalog.DefaultRewardTitle,
                catalog.ExitConfirmationTitle,
                catalog.ExitConfirmationBody,
                catalog.ExitCollectButtonLabel,
                catalog.ExitComeBackButtonLabel,
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
