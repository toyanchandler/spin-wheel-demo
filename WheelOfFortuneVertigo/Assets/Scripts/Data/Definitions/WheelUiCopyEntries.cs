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
        [SerializeField] private string _panelSpriteName;
        [SerializeField] private string _mapFrameSpriteName;
        [SerializeField] private Sprite _panelSprite;
        [SerializeField] private Sprite _mapFrameSprite;

        public ZoneType ZoneType { get { return _zoneType; } }
        public string Label { get { return _label; } }
        public string StatusHint { get { return _statusHint; } }
        public WheelUiColorKey LabelColorKey { get { return _labelColorKey; } }
        public WheelSkinTier SkinTier { get { return _skinTier; } }
        public string PanelSpriteName { get { return _panelSpriteName; } }
        public string MapFrameSpriteName { get { return _mapFrameSpriteName; } }
        public Sprite PanelSprite { get { return _panelSprite; } }
        public Sprite MapFrameSprite { get { return _mapFrameSprite; } }

        public static WheelZoneUiCopy Create(
            ZoneType zoneType,
            string label,
            string statusHint,
            WheelUiColorKey labelColorKey,
            WheelSkinTier skinTier,
            string panelSpriteName,
            string mapFrameSpriteName)
        {
            return new WheelZoneUiCopy
            {
                _zoneType = zoneType,
                _label = label,
                _statusHint = statusHint,
                _labelColorKey = labelColorKey,
                _skinTier = skinTier,
                _panelSpriteName = panelSpriteName,
                _mapFrameSpriteName = mapFrameSpriteName
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

        public WheelGamePhase Phase { get { return _phase; } }
        public string StatusMessage { get { return _statusMessage; } }
        public bool HideStatusBar { get { return _hideStatusBar; } }
        public bool HideOutcomePopup { get { return _hideOutcomePopup; } }
        public WheelPhaseGameplayProfile Gameplay { get { return _gameplay; } }

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
        [TextArea(1, 3)][SerializeField] private string _summary;
        [SerializeField] private WheelUiColorKey _resultColorKey;
        [SerializeField] private bool _showIcon;
        [SerializeField] private WheelOutcomeResultSource _resultSource;

        public WheelGamePhase Phase { get { return _phase; } }
        public string Title { get { return _title; } }
        public string ResultFallback { get { return _resultFallback; } }
        public string Summary { get { return _summary; } }
        public WheelUiColorKey ResultColorKey { get { return _resultColorKey; } }
        public bool ShowIcon { get { return _showIcon; } }
        public WheelOutcomeResultSource ResultSource { get { return _resultSource; } }

        public static WheelOutcomeUiCopy Create(
            WheelGamePhase phase,
            string title,
            string resultFallback,
            string summary,
            WheelUiColorKey resultColorKey,
            bool showIcon,
            WheelOutcomeResultSource resultSource)
        {
            return new WheelOutcomeUiCopy
            {
                _phase = phase,
                _title = title,
                _resultFallback = resultFallback,
                _summary = summary,
                _resultColorKey = resultColorKey,
                _showIcon = showIcon,
                _resultSource = resultSource
            };
        }
    }
}
