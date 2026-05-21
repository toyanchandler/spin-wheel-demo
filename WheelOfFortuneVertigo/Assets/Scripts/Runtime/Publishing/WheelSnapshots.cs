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
        public readonly string Title;
        public readonly string ResultText;
        public readonly string SummaryText;
        public readonly Color ResultColor;
        public readonly Sprite Icon;
        public readonly Color IconImageColor;
        public readonly WheelOutcomePopupMotion Motion;

        public WheelOutcomeSnapshot(
            string title,
            string resultText,
            string summaryText,
            Color resultColor,
            Sprite icon,
            Color iconImageColor,
            WheelOutcomePopupMotion motion)
        {
            Title = title;
            ResultText = resultText;
            SummaryText = summaryText;
            ResultColor = resultColor;
            Icon = icon;
            IconImageColor = iconImageColor;
            Motion = motion;
        }
    }

    public static class WheelSnapshotFactory
    {
        private static readonly Color[] OutcomeIconColors = { Color.clear, Color.white };
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

            return new WheelHudSnapshot(
                state.Zone,
                state.Phase,
                zoneCopy.label,
                theme.primaryTextColor,
                catalog.ResolveColor(zoneCopy.labelColorKey, theme),
                settings.SafeZoneInterval,
                settings.SuperZoneInterval,
                ResolveNextZone(state.Zone, settings.SafeZoneInterval),
                ResolveNextZone(state.Zone, settings.SuperZoneInterval),
                catalog.ResolveStatusMessage(state.Phase, zoneType, winLabels[hasWinLabel]),
                !catalog.ShouldHideStatusBar(state.Phase),
                theme.secondaryTextColor,
                theme.backgroundColor,
                state.CanSpin,
                state.CanLeave,
                state.CanRestart,
                !catalog.ShouldHideOutcomePopup(state.Phase),
                zoneCopy.panelSprite,
                zoneCopy.mapFrameSprite,
                layout.rewardPanelFrameSprite,
                layout.rewardCardFrameSprite,
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
                layout.sliceIconSize,
                zoneCopy.skinTier,
                settings.Theme.backgroundColor);
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
                outcomeCopy.resultSource,
                outcomeCopy.resultFallback,
                result,
                hasResult,
                inventory);
            int iconVisible = System.Convert.ToInt32(outcomeCopy.showIcon && hasResult && result.Icon != null);
            Sprite[] icons = { null, result.Icon };

            return new WheelOutcomeSnapshot(
                outcomeCopy.title,
                resultText,
                outcomeCopy.summary,
                settings.UiCopy.ResolveColor(outcomeCopy.resultColorKey, settings.Theme),
                icons[iconVisible],
                OutcomeIconColors[iconVisible],
                settings.OutcomePopupMotionCatalog.Motion);
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
