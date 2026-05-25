using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public sealed class WheelLayoutSettings
    {
        [SerializeField] private string _rewardPanelFrameSpriteName = "ui_card_frame_gardient";
        [SerializeField] private string _rewardCardFrameSpriteName = "ui_loot_row_frame_blue";
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

        public static WheelLayoutSettings Default()
        {
            return new WheelLayoutSettings();
        }
    }
}
