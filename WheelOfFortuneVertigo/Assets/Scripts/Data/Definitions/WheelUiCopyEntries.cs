using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public struct WheelZoneUiCopy
    {
        [SerializeField] private ZoneType _zoneType;
        [SerializeField] private string _label;
        [TextArea(2, 4)][SerializeField] private string _statusHint;
        [SerializeField] private WheelUiColorKey _labelColorKey;
        [SerializeField] private WheelSkinTier _skinTier;

        public ZoneType ZoneType => _zoneType;
        public string Label => _label;
        public string StatusHint => _statusHint;
        public WheelUiColorKey LabelColorKey => _labelColorKey;
        public WheelSkinTier SkinTier => _skinTier;

        public static WheelZoneUiCopy Create(
            ZoneType zoneType,
            string label,
            string statusHint,
            WheelUiColorKey labelColorKey,
            WheelSkinTier skinTier)
        {
            return new WheelZoneUiCopy
            {
                _zoneType = zoneType,
                _label = label,
                _statusHint = statusHint,
                _labelColorKey = labelColorKey,
                _skinTier = skinTier
            };
        }
    }

    [Serializable]
    public struct WheelPhaseUiCopy
    {
        [SerializeField] private WheelGamePhase _phase;
        [TextArea(1, 3)][SerializeField] private string _statusMessage;
        [SerializeField] private bool _hideStatusBar;
        [SerializeField] private bool _hideOutcomePopup;
        [SerializeField] private WheelPhaseGameplayProfile _gameplay;

        public WheelGamePhase Phase => _phase;
        public string StatusMessage => _statusMessage;
        public bool HideStatusBar => _hideStatusBar;
        public bool HideOutcomePopup => _hideOutcomePopup;
        public WheelPhaseGameplayProfile Gameplay => _gameplay;

        public static WheelPhaseUiCopy Create(
            WheelGamePhase phase,
            WheelPhaseGameplayProfile gameplay,
            string statusMessage = null,
            bool hideStatusBar = false,
            bool hideOutcomePopup = false)
        {
            return new WheelPhaseUiCopy
            {
                _phase = phase,
                _statusMessage = statusMessage,
                _hideStatusBar = hideStatusBar,
                _hideOutcomePopup = hideOutcomePopup,
                _gameplay = gameplay
            };
        }
    }

    [Serializable]
    public struct WheelOutcomeUiCopy
    {
        [SerializeField] private WheelGamePhase _phase;
        [SerializeField] private string _title;
        [SerializeField] private string _resultFallback;
        [SerializeField] private WheelUiColorKey _resultColorKey;
        [SerializeField] private bool _showIcon;
        [SerializeField] private WheelOutcomeResultSource _resultSource;

        public WheelGamePhase Phase => _phase;
        public string Title => _title;
        public string ResultFallback => _resultFallback;
        public WheelUiColorKey ResultColorKey => _resultColorKey;
        public bool ShowIcon => _showIcon;
        public WheelOutcomeResultSource ResultSource => _resultSource;

        public static WheelOutcomeUiCopy Create(
            WheelGamePhase phase,
            string title,
            string resultFallback,
            WheelUiColorKey resultColorKey,
            bool showIcon,
            WheelOutcomeResultSource resultSource)
        {
            return new WheelOutcomeUiCopy
            {
                _phase = phase,
                _title = title,
                _resultFallback = resultFallback,
                _resultColorKey = resultColorKey,
                _showIcon = showIcon,
                _resultSource = resultSource
            };
        }
    }
}
