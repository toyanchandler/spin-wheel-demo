using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public sealed class WheelLayoutSettings
    {
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);
        [SerializeField] private Vector2 _wheelPosition = new Vector2(0f, 10f);
        [SerializeField] private Vector2 _wheelSize = new Vector2(760f, 760f);
        [SerializeField] private Vector2 _indicatorPosition = new Vector2(0f, 96f);
        [SerializeField] private Vector2 _indicatorSize = new Vector2(88f, 120f);
        [SerializeField] private Vector2 _zonePanelPosition = new Vector2(0f, -86f);
        [SerializeField] private Vector2 _zonePanelSize = new Vector2(780f, 86f);
        [SerializeField] private Vector2 _rewardPanelPosition = new Vector2(-720f, -10f);
        [SerializeField] private Vector2 _rewardPanelSize = new Vector2(320f, 760f);
        [SerializeField] private Vector2 _rewardCardSize = new Vector2(248f, 72f);
        [SerializeField] private int _maxRewardCards = 8;
        [SerializeField] private string _rewardPanelFrameSpriteName = "ui_card_frame_gardient";
        [SerializeField] private string _rewardCardFrameSpriteName = "ui_card_frame_12px_neutral";
        [SerializeField] private string _zoneStandardPanelSpriteName = "ui_card_panel_zone_bg";
        [SerializeField] private string _zoneUpcomingPanelSpriteName = "ui_card_panel_zone_coming";
        [SerializeField] private string _zoneSmallFrameSpriteName = "ui_card_frame_4px_zone";
        [SerializeField] private string _starFlashSpriteName = "star_flash_alpha";
        [SerializeField] private string _starGlowSpriteName = "star_glow_alpha";
        [SerializeField] private string _shineSpriteName = "ui_vfx_offer_shine";
        [SerializeField] private Sprite _rewardPanelFrameSprite;
        [SerializeField] private Sprite _rewardCardFrameSprite;
        [SerializeField] private Sprite _zoneStandardPanelSprite;
        [SerializeField] private Sprite _zoneUpcomingPanelSprite;
        [SerializeField] private Sprite _zoneSmallFrameSprite;
        [SerializeField] private Sprite _starFlashSprite;
        [SerializeField] private Sprite _starGlowSprite;
        [SerializeField] private Sprite _shineSprite;
        [SerializeField] private Vector2 _statusPosition = new Vector2(0f, 100f);
        [SerializeField] private Vector2 _statusSize = new Vector2(900f, 48f);
        [SerializeField] private Vector2 _buttonRowPosition = new Vector2(0f, 32f);
        [SerializeField] private Vector2 _buttonSize = new Vector2(252f, 76f);
        [SerializeField] private float _buttonSpacing = 264f;
        [SerializeField, Range(0.2f, 0.48f)] private float _sliceIconRadius = 0.335f;
        [SerializeField] private Vector2 _sliceIconSize = new Vector2(92f, 92f);

        public Vector2 ReferenceResolution { get { return _referenceResolution; } }
        public Vector2 WheelPosition { get { return _wheelPosition; } }
        public Vector2 WheelSize { get { return _wheelSize; } }
        public Vector2 IndicatorPosition { get { return _indicatorPosition; } }
        public Vector2 IndicatorSize { get { return _indicatorSize; } }
        public Vector2 ZonePanelPosition { get { return _zonePanelPosition; } }
        public Vector2 ZonePanelSize { get { return _zonePanelSize; } }
        public Vector2 RewardPanelPosition { get { return _rewardPanelPosition; } }
        public Vector2 RewardPanelSize { get { return _rewardPanelSize; } }
        public Vector2 RewardCardSize { get { return _rewardCardSize; } }
        public int MaxRewardCards { get { return _maxRewardCards; } }
        public string RewardPanelFrameSpriteName { get { return _rewardPanelFrameSpriteName; } }
        public string RewardCardFrameSpriteName { get { return _rewardCardFrameSpriteName; } }
        public string ZoneStandardPanelSpriteName { get { return _zoneStandardPanelSpriteName; } }
        public string ZoneUpcomingPanelSpriteName { get { return _zoneUpcomingPanelSpriteName; } }
        public string ZoneSmallFrameSpriteName { get { return _zoneSmallFrameSpriteName; } }
        public string StarFlashSpriteName { get { return _starFlashSpriteName; } }
        public string StarGlowSpriteName { get { return _starGlowSpriteName; } }
        public string ShineSpriteName { get { return _shineSpriteName; } }
        public Sprite RewardPanelFrameSprite { get { return _rewardPanelFrameSprite; } }
        public Sprite RewardCardFrameSprite { get { return _rewardCardFrameSprite; } }
        public Sprite ZoneStandardPanelSprite { get { return _zoneStandardPanelSprite; } }
        public Sprite ZoneUpcomingPanelSprite { get { return _zoneUpcomingPanelSprite; } }
        public Sprite ZoneSmallFrameSprite { get { return _zoneSmallFrameSprite; } }
        public Sprite StarFlashSprite { get { return _starFlashSprite; } }
        public Sprite StarGlowSprite { get { return _starGlowSprite; } }
        public Sprite ShineSprite { get { return _shineSprite; } }
        public Vector2 StatusPosition { get { return _statusPosition; } }
        public Vector2 StatusSize { get { return _statusSize; } }
        public Vector2 ButtonRowPosition { get { return _buttonRowPosition; } }
        public Vector2 ButtonSize { get { return _buttonSize; } }
        public float ButtonSpacing { get { return _buttonSpacing; } }
        public float SliceIconRadius { get { return _sliceIconRadius; } }
        public Vector2 SliceIconSize { get { return _sliceIconSize; } }

        public void BindFrameSprites(Sprite rewardPanelFrameSprite, Sprite rewardCardFrameSprite)
        {
            _rewardPanelFrameSprite = rewardPanelFrameSprite;
            _rewardCardFrameSprite = rewardCardFrameSprite;
        }

        public void BindUiSprites(
            Sprite zoneStandardPanelSprite,
            Sprite zoneUpcomingPanelSprite,
            Sprite zoneSmallFrameSprite,
            Sprite starFlashSprite,
            Sprite starGlowSprite,
            Sprite shineSprite)
        {
            _zoneStandardPanelSprite = zoneStandardPanelSprite;
            _zoneUpcomingPanelSprite = zoneUpcomingPanelSprite;
            _zoneSmallFrameSprite = zoneSmallFrameSprite;
            _starFlashSprite = starFlashSprite;
            _starGlowSprite = starGlowSprite;
            _shineSprite = shineSprite;
        }

        public void ApplyCenteredComposition()
        {
            _wheelPosition = new Vector2(0f, 10f);
            _wheelSize = new Vector2(760f, 760f);
            _indicatorPosition = new Vector2(0f, 96f);
            _indicatorSize = new Vector2(88f, 120f);
            _rewardPanelPosition = new Vector2(-720f, -10f);
            _rewardPanelSize = new Vector2(320f, 760f);
            _rewardCardSize = new Vector2(248f, 72f);
            _maxRewardCards = 8;
            _statusPosition = new Vector2(0f, 100f);
            _buttonRowPosition = new Vector2(0f, 32f);
            _buttonSize = new Vector2(252f, 76f);
        }

        public static WheelLayoutSettings Default()
        {
            return new WheelLayoutSettings();
        }
    }
}
