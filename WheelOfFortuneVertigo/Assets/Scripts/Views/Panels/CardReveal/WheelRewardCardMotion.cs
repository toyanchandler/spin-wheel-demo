using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelRewardCardMotion
    {
        public const float OpeningBackHold = 0.34f;
        public const float OpeningFeaturedRevealStep = 0.14f;
        public const float OpeningFeaturedRevealMaxDelay = 2.44f;

        public const float StandardFoldDuration = 0.20f;
        public const float FeaturedFoldDuration = 0.24f;
        public const float StandardRevealDuration = 0.28f;
        public const float FeaturedRevealDuration = 0.36f;
        public const float CanvasFadeDuration = 0.10f;
        public const float FrontFadeDuration = 0.08f;
        public const float ContentFadeDuration = 0.18f;
        public const float IconScaleSettleDuration = 0.24f;
        public const float TextScaleSettleDuration = 0.20f;
        public const float CardSettleDuration = 0.18f;

        public const float HaloTintWeight = 0.36f;
        public const float FeaturedHaloPeakAlpha = 0.44f;
        public const float StandardHaloPeakAlpha = 0.26f;
        public const float FeaturedHaloSettleAlpha = 0.18f;
        public const float StandardHaloSettleAlpha = 0.10f;
        public const float HaloPeakFadeDuration = 0.16f;
        public const float HaloPeakScaleDuration = 0.28f;
        public const float HaloPeakRotateDuration = 0.34f;
        public const float HaloSettleDuration = 0.36f;

        public const float LandingPulseDelayStep = 0.02f;
        public const float LandingPulseScaleDuration = 0.12f;
        public const float LandingPulseSettleDuration = 0.22f;
        public const float LandingPulsePeakHaloAlpha = 0.38f;
        public const float LandingPulseSettleHaloAlpha = 0.16f;
        public const float LostCardAlpha = 0.62f;
        public const float RevealIdleStartPadding = 0.10f;
        public const float RevealIdleScaleDuration = 1.35f;

        public static readonly Vector3 FeaturedStartScale = new Vector3(0.90f, 0.90f, 1f);
        public static readonly Vector3 StandardStartScale = new Vector3(0.94f, 0.94f, 1f);
        public static readonly Vector3 FoldScale = new Vector3(1.05f, 1.05f, 1f);
        public static readonly Vector3 RevealPeakScale = new Vector3(1.08f, 1.08f, 1f);
        public static readonly Vector3 LandingPulseScale = new Vector3(1.12f, 1.12f, 1f);
        public static readonly Vector3 RevealIdleScale = new Vector3(1.018f, 1.018f, 1f);
        public static readonly Vector3 HaloStartScale = new Vector3(0.82f, 0.82f, 1f);
        public static readonly Vector3 HaloPeakScale = new Vector3(1.18f, 1.18f, 1f);
        public static readonly Vector3 HaloSettleScale = new Vector3(1.04f, 1.04f, 1f);
        public static readonly Vector3 IconStartScale = new Vector3(0.78f, 0.78f, 1f);
        public static readonly Vector3 TitleStartScale = new Vector3(0.94f, 0.94f, 1f);
        public static readonly Vector3 AmountStartScale = new Vector3(0.90f, 0.90f, 1f);

        public static readonly Vector3 StandardStartRotation = new Vector3(0f, -78f, -1.5f);
        public static readonly Vector3 FeaturedStartRotation = new Vector3(0f, -78f, -3.5f);
        public static readonly Vector3 StandardFoldRotation = new Vector3(0f, 88f, 1.5f);
        public static readonly Vector3 FeaturedFoldRotation = new Vector3(0f, 88f, 2.5f);
        public static readonly Vector3 FlipFrontRotation = new Vector3(0f, -88f, 1.5f);
        public static readonly Vector3 StandardRevealRotation = new Vector3(0f, -8f, -0.3f);
        public static readonly Vector3 FeaturedRevealRotation = new Vector3(0f, -8f, -0.8f);
        public static readonly Vector3 HaloPeakRotation = new Vector3(0f, 0f, 72f);
        public static readonly Vector3 HaloSettleRotation = new Vector3(0f, 0f, 118f);

        public static readonly Color LostFrontColor = new Color(0.17f, 0.075f, 0.058f, 0.70f);
        public static readonly Color LostHaloColor = new Color(1f, 0.22f, 0.06f, 0.12f);

        public static float RevealDelay(int displayIndex)
        {
            return Mathf.Min(OpeningFeaturedRevealMaxDelay, OpeningBackHold + (displayIndex * OpeningFeaturedRevealStep));
        }

        public static float RevealIdleDelay(int displayIndex, int visibleCardCount)
        {
            int lastDisplayIndex = Mathf.Max(0, visibleCardCount - 1);
            return Mathf.Max(0f, RevealDelay(lastDisplayIndex) - RevealDelay(displayIndex)) + RevealIdleStartPadding;
        }
    }
}
