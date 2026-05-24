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
        public readonly Sprite ZonePanelSprite;
        public readonly Sprite ZoneMapFrameSprite;
        public readonly Sprite RewardPanelFrameSprite;
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
            Sprite zonePanelSprite,
            Sprite zoneMapFrameSprite,
            Sprite rewardPanelFrameSprite,
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
            ZonePanelSprite = zonePanelSprite;
            ZoneMapFrameSprite = zoneMapFrameSprite;
            RewardPanelFrameSprite = rewardPanelFrameSprite;
            RewardCardFrameSprite = rewardCardFrameSprite;
            RewardCardCount = rewardCardCount;
            RewardCards = rewardCards;
        }
    }

    public readonly struct WheelZoneSnapshot
    {
        public readonly WheelSliceDefinition[] Slices;
        public readonly int SliceCount;
        public readonly Vector2[] SlicePositions;
        public readonly Vector2 SliceIconSize;
        public readonly WheelSkinTier SkinTier;
        public readonly Color BackgroundColor;

        public WheelZoneSnapshot(
            WheelSliceDefinition[] slices,
            int sliceCount,
            Vector2[] slicePositions,
            Vector2 sliceIconSize,
            WheelSkinTier skinTier,
            Color backgroundColor)
        {
            Slices = slices;
            SliceCount = sliceCount;
            SlicePositions = slicePositions;
            SliceIconSize = sliceIconSize;
            SkinTier = skinTier;
            BackgroundColor = backgroundColor;
        }
    }

    public readonly struct WheelOutcomeSnapshot
    {
        public readonly WheelGamePhase Phase;
        public readonly string Title;
        public readonly string ResultText;
        public readonly string SummaryText;
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
            string summaryText,
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
            SummaryText = summaryText;
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
        private static RewardInventoryEntry[] _rewardCardBuffer = new RewardInventoryEntry[16];

        public static WheelHudSnapshot CreateHud(WheelGameState state)
        {
            WheelGameSettings settings = state.Settings;
            WheelUiCopyCatalog catalog = settings.UiCopy;
            WheelThemeSettings theme = settings.Theme;
            WheelLayoutSettings layout = settings.Layout;
            ZoneType zoneType = state.ZoneType;
            WheelZoneUiCopy zoneCopy = catalog.GetZoneCopy(zoneType);
            int hasWinLabel = System.Convert.ToInt32(state.HasLastResult);
            string[] winLabels = { null, state.LastResult.WinLabel };
            int rewardCardCount = state.Inventory.CopyEntries(_rewardCardBuffer);
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
                catalog.ResolveStatusMessage(state.Phase, zoneType, winLabels[hasWinLabel]),
                !catalog.ShouldHideStatusBar(state.Phase),
                theme.SecondaryTextColor,
                theme.BackgroundColor,
                state.CanSpin,
                state.CanLeave,
                state.CanRestart,
                !catalog.ShouldHideOutcomePopup(state.Phase),
                zoneCopy.PanelSprite,
                zoneCopy.MapFrameSprite,
                layout.RewardPanelFrameSprite,
                layout.RewardCardFrameSprite,
                rewardCardCount,
                _rewardCardBuffer);
        }

        private static int ResolveNextZone(int currentZone, int interval)
        {
            int safeInterval = Mathf.Max(1, interval);
            int remainder = currentZone % safeInterval;
            int[] offsets = { safeInterval - remainder, 0 };
            return currentZone + offsets[System.Convert.ToInt32(remainder == 0)];
        }

        public static WheelZoneSnapshot CreateZone(WheelGameState state)
        {
            WheelGameSettings settings = state.Settings;
            WheelLayoutSettings layout = settings.Layout;
            WheelZoneUiCopy zoneCopy = settings.UiCopy.GetZoneCopy(state.ZoneType);
            Vector2[] slicePositions = WheelSliceLayoutResolver.BuildPositions(
                settings.SliceLayoutCatalog,
                layout,
                state.SliceCount);

            return new WheelZoneSnapshot(
                state.Slices,
                state.SliceCount,
                slicePositions,
                layout.SliceIconSize,
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
                inventory);
            string title = outcomeCopy.Title;
            string summary = outcomeCopy.Summary;
            ApplyRewardOutcomeCopy(phase, result, hasResult, ref title, ref resultText, ref summary);
            int iconVisible = System.Convert.ToInt32(outcomeCopy.ShowIcon && hasResult && result.Icon != null);
            Sprite[] icons = { null, result.Icon };
            Color[] iconColors = { Color.clear, ResolveOutcomeIconColor(result, hasResult) };
            bool hasPresentableOutcome = hasResult && iconVisible == 1;

            return new WheelOutcomeSnapshot(
                phase,
                title,
                resultText,
                summary,
                settings.UiCopy.ResolveColor(outcomeCopy.ResultColorKey, settings.Theme),
                icons[iconVisible],
                iconColors[iconVisible],
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

        private static void ApplyRewardOutcomeCopy(
            WheelGamePhase phase,
            WheelSpinResult result,
            bool hasResult,
            ref string title,
            ref string resultText,
            ref string summary)
        {
            if (phase != WheelGamePhase.Won || !hasResult || result.IsBomb)
            {
                return;
            }

            string displayName = string.IsNullOrEmpty(result.DisplayName) ? resultText : result.DisplayName;
            title = "YOU GOT";
            resultText = result.Amount > 1 ? string.Format("{0} x{1}", displayName, result.Amount) : displayName;
            summary = "Added to your stash";
        }

        private static string ResolveOutcomeResultText(
            WheelOutcomeResultSource resultSource,
            string resultFallback,
            WheelSpinResult result,
            bool hasResult,
            RewardInventory inventory)
        {
            int hasSpinResult = System.Convert.ToInt32(hasResult);
            string[] values =
            {
                hasSpinResult == 1 ? result.WinLabel : resultFallback,
                inventory.BuildSummary(),
                resultFallback
            };

            return values[(int)resultSource];
        }
    }
}
