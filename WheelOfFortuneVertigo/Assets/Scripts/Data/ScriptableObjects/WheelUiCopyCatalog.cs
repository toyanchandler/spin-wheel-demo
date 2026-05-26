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
        [SerializeField] private string _spinButtonLabel = "SPIN";
        [SerializeField] private string _leaveButtonLabel = "EXIT";
        [SerializeField] private string _restartButtonLabel = "RETRY RUN";
        [SerializeField] private string _rewardOpeningTitle = "YOUR REWARDS:";
        [SerializeField] private string _defaultRewardTitle = "REWARD";
        [SerializeField] private string _inventoryEmptySummary = "No rewards";
        [SerializeField] private string _inventoryFallbackRewardName = "Reward";
        [SerializeField] private string _exitConfirmationTitle = "COLLECT REWARDS?";
        [SerializeField] private string _exitConfirmationBody = "Are you sure you want to go out and collect your rewards? We have saved the best rewards for last!";
        [SerializeField] private string _exitCollectButtonLabel = "COLLECT REWARDS";
        [SerializeField] private string _exitComeBackButtonLabel = "COME BACK";
        [SerializeField] private WheelZoneUiCopy[] _zones = CreateDefaultZones();
        [SerializeField] private WheelPhaseUiCopy[] _phases = CreateDefaultPhases();
        [SerializeField] private WheelOutcomeUiCopy[] _outcomes = CreateDefaultOutcomes();

        private WheelZoneUiCopy[] _zoneByType;
        private WheelPhaseUiCopy[] _phaseByType;
        private WheelOutcomeUiCopy[] _outcomeByPhase;

        public string WinLabelFormat { get { return _winLabelFormat; } }
        public string SafeMilestoneBadgeFormat { get { return _safeMilestoneBadgeFormat; } }
        public string SuperMilestoneBadgeFormat { get { return _superMilestoneBadgeFormat; } }
        public string SpinButtonLabel { get { return _spinButtonLabel; } }
        public string LeaveButtonLabel { get { return _leaveButtonLabel; } }
        public string RestartButtonLabel { get { return _restartButtonLabel; } }
        public string RewardOpeningTitle { get { return _rewardOpeningTitle; } }
        public string DefaultRewardTitle { get { return _defaultRewardTitle; } }
        public string InventoryEmptySummary { get { return _inventoryEmptySummary; } }
        public string InventoryFallbackRewardName { get { return _inventoryFallbackRewardName; } }
        public string ExitConfirmationTitle { get { return _exitConfirmationTitle; } }
        public string ExitConfirmationBody { get { return _exitConfirmationBody; } }
        public string ExitCollectButtonLabel { get { return _exitCollectButtonLabel; } }
        public string ExitComeBackButtonLabel { get { return _exitComeBackButtonLabel; } }

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
            _safeMilestoneBadgeFormat = "SAFE\nZONE\n{0}";
            _superMilestoneBadgeFormat = "SUPER\nZONE\n{0}";
            _spinButtonLabel = "SPIN";
            _leaveButtonLabel = "EXIT";
            _restartButtonLabel = "RETRY RUN";
            _rewardOpeningTitle = "YOUR REWARDS:";
            _defaultRewardTitle = "REWARD";
            _inventoryEmptySummary = "No rewards";
            _inventoryFallbackRewardName = "Reward";
            _exitConfirmationTitle = "COLLECT REWARDS?";
            _exitConfirmationBody = "Are you sure you want to go out and collect your rewards? We have saved the best rewards for last!";
            _exitCollectButtonLabel = "COLLECT REWARDS";
            _exitComeBackButtonLabel = "COME BACK";
            _zones = CreateDefaultZones();
            _phases = CreateDefaultPhases();
            _outcomes = CreateDefaultOutcomes();
            RebuildLookups();
        }

        public void RefreshLookups()
        {
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
                    "Risk your loot. Reach the safe zone.",
                    WheelUiColorKey.StandardZone,
                    WheelSkinTier.Bronze),
                WheelZoneUiCopy.Create(
                    ZoneType.Safe,
                    "SAFE",
                    "Safe zone. Spin without a bomb or leave with your loot.",
                    WheelUiColorKey.SafeZone,
                    WheelSkinTier.Silver),
                WheelZoneUiCopy.Create(
                    ZoneType.Super,
                    "SUPER",
                    "Super zone. Spin for premium rewards or leave with your loot.",
                    WheelUiColorKey.SuperZone,
                    WheelSkinTier.Golden)
            };
        }

        private static WheelPhaseUiCopy[] CreateDefaultPhases()
        {
            return new[]
            {
                WheelPhaseUiCopy.Create(
                    WheelGamePhase.Ready,
                    WheelPhaseGameplayProfile.Create(allowSpin: true, allowLeave: true, publishAllAfterSpin: true),
                    hideOutcomePopup: true),
                WheelPhaseUiCopy.Create(
                    WheelGamePhase.Spinning,
                    WheelPhaseGameplayProfile.Create(),
                    statusMessage: "Spinning...",
                    hideOutcomePopup: true),
                WheelPhaseUiCopy.Create(
                    WheelGamePhase.Won,
                    WheelPhaseGameplayProfile.Create(allowSpin: true, allowLeave: true, publishAllAfterSpin: true, useWinLabelForStatus: true),
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
                    WheelUiColorKey.PrimaryText,
                    true,
                    WheelOutcomeResultSource.SpinResultLabel),
                WheelOutcomeUiCopy.Create(
                    WheelGamePhase.Bombed,
                    "RUN LOST",
                    "BOOM! BUSTED",
                    WheelUiColorKey.Danger,
                    true,
                    WheelOutcomeResultSource.StaticFallback),
                WheelOutcomeUiCopy.Create(
                    WheelGamePhase.CashedOut,
                    "CASHED OUT",
                    "Cashed out",
                    WheelUiColorKey.Success,
                    false,
                    WheelOutcomeResultSource.InventorySummary)
            };
        }
    }
}
