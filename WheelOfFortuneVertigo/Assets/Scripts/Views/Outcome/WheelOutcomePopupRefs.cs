using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelOutcomePopupRefs
    {
        public GameObject Root;
        public Image IconImage;
        public TextMeshProUGUI ResultText;
        public TextMeshProUGUI SummaryText;
        public CanvasGroup CanvasGroup;
        public RectTransform ContentRoot;
        public CanvasGroup RewardChromeGroup;
        public GameObject RewardPopupBackground;
        public GameObject BombCardShadow;
        public GameObject BombWarmTopGlow;
        public GameObject BombHalo;
        public GameObject OutcomeRetryButton;
        public Image FlashImage;
        public Image ShineImage;
        public Image[] FlightIconPool;
        public Camera RewardBurstCamera;
        public RawImage RewardBurstDisplay;
        public ParticleSystem RewardBurstParticle;
        public ParticleSystemRenderer RewardBurstRenderer;
        public WheelRewardPanelView RewardPanelView;
        public Vector2 IconHomeAnchoredPosition;
        public Vector2 ContentHomeAnchorMin;
        public Vector2 ContentHomeAnchorMax;
        public Vector2 ContentHomePivot;
        public Vector2 ContentHomeAnchoredPosition;
        public Vector3 ContentHomeScale;
        public Func<WheelOutcomeSnapshot> GetCurrentSnapshot;
        public Action MarkPresentationComplete;
    }
}
