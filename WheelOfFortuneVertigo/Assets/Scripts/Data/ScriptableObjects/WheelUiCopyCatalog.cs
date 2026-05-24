using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    public enum WheelUiColorKey
    {
        PrimaryText,
        SecondaryText,
        StandardZone,
        SafeZone,
        SuperZone,
        Danger,
        Success
    }

    public enum WheelSkinTier
    {
        Bronze,
        Silver,
        Golden
    }

    [CreateAssetMenu(fileName = "WheelUiCopyCatalog", menuName = "Vertigo/Wheel UI Copy Catalog")]
    public sealed class WheelUiCopyCatalog : ScriptableObject
    {
        private const int ZoneTypeCount = 3;
        private const int PhaseCount = 5;

        [SerializeField] private string _winLabelFormat = "Won {0}";
        [SerializeField] private string _safeMilestoneBadgeFormat = "SAFE\nZONE\n{0}";
        [SerializeField] private string _superMilestoneBadgeFormat = "SUPER\nZONE\n{0}";
        [SerializeField] private WheelZoneUiCopy[] _zones = CreateDefaultZones();
        [SerializeField] private WheelPhaseUiCopy[] _phases = CreateDefaultPhases();
        [SerializeField] private WheelOutcomeUiCopy[] _outcomes = CreateDefaultOutcomes();

        private WheelZoneUiCopy[] _zoneByType;
        private WheelPhaseUiCopy[] _phaseByType;
        private WheelOutcomeUiCopy[] _outcomeByPhase;

        public string WinLabelFormat { get { return _winLabelFormat; } }
        public string SafeMilestoneBadgeFormat { get { return _safeMilestoneBadgeFormat; } }
        public string SuperMilestoneBadgeFormat { get { return _superMilestoneBadgeFormat; } }

        private void OnEnable()
        {
            RebuildLookups();
        }

        public WheelZoneUiCopy GetZoneCopy(ZoneType zoneType)
        {
            return _zoneByType[(int)zoneType];
        }

        public WheelPhaseUiCopy GetPhaseCopy(WheelGamePhase phase)
        {
            return _phaseByType[(int)phase];
        }

        public WheelOutcomeUiCopy GetOutcomeCopy(WheelGamePhase phase)
        {
            return _outcomeByPhase[(int)phase];
        }

        public bool ShouldHideStatusBar(WheelGamePhase phase)
        {
            return GetPhaseCopy(phase).HideStatusBar;
        }

        public bool ShouldHideOutcomePopup(WheelGamePhase phase)
        {
            return GetPhaseCopy(phase).HideOutcomePopup;
        }

        public string ResolveStatusMessage(WheelGamePhase phase, ZoneType zoneType, string winLabel)
        {
            WheelPhaseUiCopy phaseCopy = GetPhaseCopy(phase);
            if (!string.IsNullOrEmpty(phaseCopy.StatusMessage))
            {
                string[] messages = { phaseCopy.StatusMessage, winLabel };
                return messages[System.Convert.ToInt32(phaseCopy.Gameplay.UseWinLabelForStatus)];
            }

            return GetZoneCopy(zoneType).StatusHint;
        }

        public Color ResolveColor(WheelUiColorKey colorKey, WheelThemeSettings theme)
        {
            return theme == null ? Color.white : theme.GetUiColor(colorKey);
        }

        public string FormatWinLabel(string label)
        {
            return string.Format(_winLabelFormat, label);
        }

        public void ResetToDefaults()
        {
            _winLabelFormat = "Won {0}";
            _zones = CreateDefaultZones();
            _phases = CreateDefaultPhases();
            _outcomes = CreateDefaultOutcomes();
            RebuildLookups();
        }

        private void RebuildLookups()
        {
            _zoneByType = WheelEnumLookup.Build(_zones, entry => (int)entry.ZoneType, ZoneTypeCount);
            _phaseByType = WheelEnumLookup.Build(_phases, entry => (int)entry.Phase, PhaseCount);
            _outcomeByPhase = WheelEnumLookup.Build(_outcomes, entry => (int)entry.Phase, PhaseCount);
        }

        private static WheelZoneUiCopy[] CreateDefaultZones()
        {
            return new[]
            {
                WheelZoneUiCopy.Create(
                    ZoneType.Standard,
                    "STANDARD",
                    "Spin to risk your loot and advance toward safe zones.",
                    WheelUiColorKey.StandardZone,
                    WheelSkinTier.Bronze,
                    "ui_card_panel_zone_current",
                    "ui_card_zone_map_frame"),
                WheelZoneUiCopy.Create(
                    ZoneType.Safe,
                    "SAFE",
                    "Safe zone. Spin without a bomb or leave with your loot.",
                    WheelUiColorKey.SafeZone,
                    WheelSkinTier.Silver,
                    "ui_card_panel_zone_current_white",
                    "ui_card_zone_map_frame"),
                WheelZoneUiCopy.Create(
                    ZoneType.Super,
                    "SUPER",
                    "Super zone. Spin for premium rewards or leave with your loot.",
                    WheelUiColorKey.SuperZone,
                    WheelSkinTier.Golden,
                    "ui_card_panel_zone_super",
                    "ui_card_zone_map_frame")
            };
        }

        private static WheelPhaseUiCopy[] CreateDefaultPhases()
        {
            return new[]
            {
                WheelPhaseUiCopy.Create(
                    WheelGamePhase.Ready,
                    WheelPhaseGameplayProfile.Create(allowSpin: true, allowLeave: true, allowRestart: true, publishAllAfterSpin: true),
                    hideOutcomePopup: true),
                WheelPhaseUiCopy.Create(
                    WheelGamePhase.Spinning,
                    WheelPhaseGameplayProfile.Create(),
                    statusMessage: "Spinning...",
                    hideOutcomePopup: true),
                WheelPhaseUiCopy.Create(
                    WheelGamePhase.Won,
                    WheelPhaseGameplayProfile.Create(allowSpin: true, allowLeave: true, allowRestart: true, publishAllAfterSpin: true, useWinLabelForStatus: true),
                    hideStatusBar: true),
                WheelPhaseUiCopy.Create(
                    WheelGamePhase.Bombed,
                    WheelPhaseGameplayProfile.Create(allowRestart: true),
                    hideStatusBar: true),
                WheelPhaseUiCopy.Create(
                    WheelGamePhase.CashedOut,
                    WheelPhaseGameplayProfile.Create(allowRestart: true),
                    hideStatusBar: true)
            };
        }

        private static WheelOutcomeUiCopy[] CreateDefaultOutcomes()
        {
            return new[]
            {
                WheelOutcomeUiCopy.Create(
                    WheelGamePhase.Won,
                    "YOU GOT",
                    "Reward",
                    "Added to your stash",
                    WheelUiColorKey.PrimaryText,
                    true,
                    WheelOutcomeResultSource.SpinResultLabel),
                WheelOutcomeUiCopy.Create(
                    WheelGamePhase.Bombed,
                    "BOMB",
                    "Rewards lost",
                    "Try again",
                    WheelUiColorKey.Danger,
                    true,
                    WheelOutcomeResultSource.SpinResultLabel),
                WheelOutcomeUiCopy.Create(
                    WheelGamePhase.CashedOut,
                    "CASHED OUT",
                    "Cashed out",
                    "Rewards secured",
                    WheelUiColorKey.Success,
                    false,
                    WheelOutcomeResultSource.InventorySummary)
            };
        }
    }
}
