using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public sealed class WheelLayoutSettings
    {
        public Vector2 referenceResolution = new Vector2(1920f, 1080f);
        public Vector2 wheelPosition = new Vector2(110f, -8f);
        public Vector2 wheelSize = new Vector2(620f, 620f);
        public Vector2 indicatorPosition = new Vector2(110f, 340f);
        public Vector2 indicatorSize = new Vector2(72f, 98f);
        public Vector2 zonePanelPosition = new Vector2(0f, -86f);
        public Vector2 zonePanelSize = new Vector2(780f, 86f);
        public Vector2 rewardPanelPosition = new Vector2(-742f, -22f);
        public Vector2 rewardPanelSize = new Vector2(282f, 692f);
        public Vector2 rewardCardSize = new Vector2(214f, 64f);
        public int maxRewardCards = 8;
        public string rewardPanelFrameSpriteName = "ui_card_frame_gardient";
        public string rewardCardFrameSpriteName = "ui_card_frame_12px_neutral";
        public Sprite rewardPanelFrameSprite;
        public Sprite rewardCardFrameSprite;
        public Vector2 statusPosition = new Vector2(110f, 108f);
        public Vector2 statusSize = new Vector2(900f, 56f);
        public Vector2 buttonRowPosition = new Vector2(110f, 32f);
        public Vector2 buttonSize = new Vector2(236f, 72f);
        public float buttonSpacing = 264f;
        [Range(0.2f, 0.48f)] public float sliceIconRadius = 0.335f;
        public Vector2 sliceIconSize = new Vector2(92f, 92f);

        public static WheelLayoutSettings Default()
        {
            return new WheelLayoutSettings();
        }
    }
}
