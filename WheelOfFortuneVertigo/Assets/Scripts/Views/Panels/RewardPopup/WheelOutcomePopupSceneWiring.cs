using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    /// <summary>
    /// Flat inspector wiring for the outcome popup — one struct, no per-child binding MonoBehaviours.
    /// Same pattern as <see cref="WheelRewardOpeningBindings"/> / <see cref="WheelExitConfirmationBindings"/>.
    /// </summary>
    [Serializable]
    public sealed class WheelOutcomePopupSceneWiring
    {
        [Header("Structure")]
        public CanvasGroup RootCanvas;
        public Image RootOverlay;
        public RectTransform ContentRoot;

        [Header("Content")]
        public Image Icon;
        public TextMeshProUGUI ResultText;
        public CanvasGroup Chrome;
        public GameObject RewardPopupBackground;
        public GameObject BombCardShadow;
        public GameObject OutcomeRetryButton;

        [Header("Reveal FX")]
        public Image Flash;
        public Image Shine;
        public Image FlightIcon;

        [Header("Reward Burst")]
        public Camera RewardBurstCamera;
        public RawImage RewardBurstDisplay;
        public ParticleSystem RewardBurstParticle;

        public bool HasRoot => RootCanvas != null;
    }
}
