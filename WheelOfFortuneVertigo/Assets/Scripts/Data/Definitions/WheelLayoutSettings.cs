using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public sealed class WheelLayoutSettings
    {
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);
        [SerializeField] private Vector2 _wheelPosition = new Vector2(110f, -8f);
        [SerializeField] private Vector2 _wheelSize = new Vector2(620f, 620f);
        [SerializeField] private Vector2 _indicatorPosition = new Vector2(110f, 340f);
        [SerializeField] private Vector2 _indicatorSize = new Vector2(72f, 98f);
        [SerializeField] private Vector2 _zonePanelPosition = new Vector2(0f, -86f);
        [SerializeField] private Vector2 _zonePanelSize = new Vector2(780f, 86f);
        [SerializeField] private Vector2 _rewardPanelPosition = new Vector2(-742f, -22f);
        [SerializeField] private Vector2 _rewardPanelSize = new Vector2(282f, 692f);
        [SerializeField] private Vector2 _rewardCardSize = new Vector2(214f, 64f);
        [SerializeField] private int _maxRewardCards = 8;
        [SerializeField] private string _rewardPanelFrameSpriteName = "ui_card_frame_gardient";
        [SerializeField] private string _rewardCardFrameSpriteName = "ui_card_frame_12px_neutral";
        [SerializeField] private Sprite _rewardPanelFrameSprite;
        [SerializeField] private Sprite _rewardCardFrameSprite;
        [SerializeField] private Vector2 _statusPosition = new Vector2(110f, 108f);
        [SerializeField] private Vector2 _statusSize = new Vector2(900f, 56f);
        [SerializeField] private Vector2 _buttonRowPosition = new Vector2(110f, 32f);
        [SerializeField] private Vector2 _buttonSize = new Vector2(236f, 72f);
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
        public Sprite RewardPanelFrameSprite { get { return _rewardPanelFrameSprite; } }
        public Sprite RewardCardFrameSprite { get { return _rewardCardFrameSprite; } }
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

        public static WheelLayoutSettings Default()
        {
            return new WheelLayoutSettings();
        }
    }
}
