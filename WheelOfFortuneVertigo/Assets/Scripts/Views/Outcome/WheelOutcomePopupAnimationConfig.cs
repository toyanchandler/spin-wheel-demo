using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelOutcomePopupAnimationConfig
    {
        public const float PopupFadeInDuration = 0.09f;
        public const float ChromeFadeInDuration = 0.14f;
        public const float RevealStageStart = 0.32f;
        public const float RewardBurstStopTime = 3.6f;
        public const float RewardHoldDuration = 1.86f;
        public const float RewardFlightDelay = 0.07f;
        public const float RewardFlightDuration = 0.72f;
        public const float PopupFadeOutDuration = 0.12f;
        public const float IconFadeOutDuration = 0.08f;
        public const float IconPreviewFadeDuration = 0.08f;
        public const float IconFinalFadeDuration = 0.08f;
        public const float TextFadeInDelay = 0.18f;
        public const float TextFadeInDuration = 0.18f;
        public const float SummaryFadeBeforeFlight = 0.18f;
        public const float FlashFadeInDuration = 0.16f;
        public const float FlashFadeOutDuration = 0.34f;
        public const float ShineFadeInDuration = 0.12f;
        public const float ShineHoldDuration = 0.18f;
        public const float ShineFadeOutDuration = 0.18f;
        public const float ShineAlpha = 0.64f;
        public const float IconPreviewAlpha = 0.92f;
        public const float RewardFlightRotation = 10f;
        public const float RewardFlightFadeRatio = 0.68f;
        public const float BombFlashAlpha = 0.22f;
        public const float RewardFlashAlpha = 0.34f;

        public static readonly Vector3 IconStartScale = new Vector3(0.38f, 0.38f, 1f);
        public static readonly Vector3 IconRevealScale = new Vector3(0.58f, 0.58f, 1f);
        public static readonly Vector3 IconRewardPeakScale = new Vector3(1.02f, 1.02f, 1f);
        public static readonly Vector3 IconRewardSettleScale = new Vector3(0.92f, 0.92f, 1f);
        public static readonly Vector3 IconFlightEndScale = new Vector3(0.42f, 0.42f, 1f);
    }
}
