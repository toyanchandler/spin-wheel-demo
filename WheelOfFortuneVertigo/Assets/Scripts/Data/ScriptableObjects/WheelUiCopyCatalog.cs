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

    [Serializable]
    public struct WheelZoneUiCopy
    {
        public ZoneType zoneType;
        public string label;
        [TextArea(2, 4)] public string statusHint;
        public WheelUiColorKey labelColorKey;
        public WheelSkinTier skinTier;
        public string panelSpriteName;
        public string mapFrameSpriteName;
        public Sprite panelSprite;
        public Sprite mapFrameSprite;
    }

    [Serializable]
    public struct WheelPhaseUiCopy
    {
        public WheelGamePhase phase;
        [TextArea(1, 3)] public string statusMessage;
        public bool hideStatusBar;
        public bool hideOutcomePopup;
        public WheelPhaseGameplayProfile gameplay;
    }

    [Serializable]
    public struct WheelOutcomeUiCopy
    {
        public WheelGamePhase phase;
        public string title;
        public string resultFallback;
        [TextArea(1, 3)] public string summary;
        public WheelUiColorKey resultColorKey;
        public bool showIcon;
        public WheelOutcomeResultSource resultSource;
    }

    [CreateAssetMenu(fileName = "WheelUiCopyCatalog", menuName = "Vertigo/Wheel UI Copy Catalog")]
    public sealed class WheelUiCopyCatalog : ScriptableObject
    {
        private const int ZoneTypeCount = 3;
        private const int PhaseCount = 5;

        [SerializeField] private string _winLabelFormat = "Won {0}";
        [SerializeField] private WheelZoneUiCopy[] _zones = CreateDefaultZones();
        [SerializeField] private WheelPhaseUiCopy[] _phases = CreateDefaultPhases();
        [SerializeField] private WheelOutcomeUiCopy[] _outcomes = CreateDefaultOutcomes();

        private WheelZoneUiCopy[] _zoneByType;
        private WheelPhaseUiCopy[] _phaseByType;
        private WheelOutcomeUiCopy[] _outcomeByPhase;

        public string WinLabelFormat { get { return _winLabelFormat; } }

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
            return GetPhaseCopy(phase).hideStatusBar;
        }

        public bool ShouldHideOutcomePopup(WheelGamePhase phase)
        {
            return GetPhaseCopy(phase).hideOutcomePopup;
        }

        public string ResolveStatusMessage(WheelGamePhase phase, ZoneType zoneType, string winLabel)
        {
            WheelPhaseUiCopy phaseCopy = GetPhaseCopy(phase);
            if (!string.IsNullOrEmpty(phaseCopy.statusMessage))
            {
                string[] messages = { phaseCopy.statusMessage, winLabel };
                return messages[System.Convert.ToInt32(phaseCopy.gameplay.useWinLabelForStatus)];
            }

            return GetZoneCopy(zoneType).statusHint;
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
            _zoneByType = WheelEnumLookup.Build(_zones, entry => (int)entry.zoneType, ZoneTypeCount);
            _phaseByType = WheelEnumLookup.Build(_phases, entry => (int)entry.phase, PhaseCount);
            _outcomeByPhase = WheelEnumLookup.Build(_outcomes, entry => (int)entry.phase, PhaseCount);
        }

        private static WheelZoneUiCopy[] CreateDefaultZones()
        {
            return new[]
            {
                new WheelZoneUiCopy
                {
                    zoneType = ZoneType.Standard,
                    label = "STANDARD",
                    statusHint = "Spin to risk your loot and advance toward safe zones.",
                    labelColorKey = WheelUiColorKey.StandardZone,
                    skinTier = WheelSkinTier.Bronze,
                    panelSpriteName = "ui_card_panel_zone_current",
                    mapFrameSpriteName = "ui_card_zone_map_frame"
                },
                new WheelZoneUiCopy
                {
                    zoneType = ZoneType.Safe,
                    label = "SAFE",
                    statusHint = "Safe zone. Spin without a bomb or leave with your loot.",
                    labelColorKey = WheelUiColorKey.SafeZone,
                    skinTier = WheelSkinTier.Silver,
                    panelSpriteName = "ui_card_panel_zone_current_white",
                    mapFrameSpriteName = "ui_card_zone_map_frame"
                },
                new WheelZoneUiCopy
                {
                    zoneType = ZoneType.Super,
                    label = "SUPER",
                    statusHint = "Super zone. Spin for premium rewards or leave with your loot.",
                    labelColorKey = WheelUiColorKey.SuperZone,
                    skinTier = WheelSkinTier.Golden,
                    panelSpriteName = "ui_card_panel_zone_super",
                    mapFrameSpriteName = "ui_card_zone_map_frame"
                }
            };
        }

        private static WheelPhaseUiCopy[] CreateDefaultPhases()
        {
            return new[]
            {
                new WheelPhaseUiCopy
                {
                    phase = WheelGamePhase.Ready,
                    hideOutcomePopup = true,
                    gameplay = new WheelPhaseGameplayProfile
                    {
                        allowSpin = true,
                        allowLeave = true,
                        allowRestart = true,
                        publishAllAfterSpin = true
                    }
                },
                new WheelPhaseUiCopy
                {
                    phase = WheelGamePhase.Spinning,
                    statusMessage = "Spinning...",
                    hideOutcomePopup = true,
                    gameplay = new WheelPhaseGameplayProfile()
                },
                new WheelPhaseUiCopy
                {
                    phase = WheelGamePhase.Won,
                    hideStatusBar = true,
                    gameplay = new WheelPhaseGameplayProfile
                    {
                        allowSpin = true,
                        allowLeave = true,
                        allowRestart = true,
                        publishAllAfterSpin = true,
                        useWinLabelForStatus = true
                    }
                },
                new WheelPhaseUiCopy
                {
                    phase = WheelGamePhase.Bombed,
                    hideStatusBar = true,
                    gameplay = new WheelPhaseGameplayProfile
                    {
                        allowRestart = true
                    }
                },
                new WheelPhaseUiCopy
                {
                    phase = WheelGamePhase.CashedOut,
                    hideStatusBar = true,
                    gameplay = new WheelPhaseGameplayProfile
                    {
                        allowRestart = true
                    }
                }
            };
        }

        private static WheelOutcomeUiCopy[] CreateDefaultOutcomes()
        {
            return new[]
            {
                new WheelOutcomeUiCopy
                {
                    phase = WheelGamePhase.Won,
                    title = "REWARD",
                    resultFallback = "Won",
                    summary = "Added to your run rewards",
                    resultColorKey = WheelUiColorKey.PrimaryText,
                    showIcon = true,
                    resultSource = WheelOutcomeResultSource.SpinResultLabel
                },
                new WheelOutcomeUiCopy
                {
                    phase = WheelGamePhase.Bombed,
                    title = "BOMB",
                    resultFallback = "Rewards lost",
                    summary = "Try again",
                    resultColorKey = WheelUiColorKey.Danger,
                    showIcon = true,
                    resultSource = WheelOutcomeResultSource.SpinResultLabel
                },
                new WheelOutcomeUiCopy
                {
                    phase = WheelGamePhase.CashedOut,
                    title = "CASHED OUT",
                    resultFallback = "Cashed out",
                    summary = "Rewards secured",
                    resultColorKey = WheelUiColorKey.Success,
                    showIcon = false,
                    resultSource = WheelOutcomeResultSource.InventorySummary
                }
            };
        }
    }
}
