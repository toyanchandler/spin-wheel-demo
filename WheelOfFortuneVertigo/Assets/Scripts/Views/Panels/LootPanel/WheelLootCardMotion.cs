using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelLootCardMotion
    {
        public const float LostCardAlpha = 0.62f;
        public const float PreparedGlowAlpha = 0.12f;
        public const float RestingGlowAlpha = 0.10f;
        public const float RestingShadowAlpha = 0.10f;
        public const float MaxEntranceDelay = 0.42f;
        public const float EntranceDelayStep = 0.025f;
        public const float EntranceFadeDuration = 0.12f;
        public const float EntranceScaleDuration = 0.20f;
        public const float EntranceRotateDuration = 0.18f;
        public const float IconEntranceScaleDuration = 0.18f;
        public const float EntranceShimmerDelay = 0f;
        public const float EntranceShimmerDuration = 0.28f;
        public const float LandingPulseShimmerDelay = 0.02f;
        public const float LandingPulseShimmerDuration = 1.08f;
        public const float LandingPulseScaleDuration = 0.12f;
        public const float LandingPulseSettleDuration = 0.20f;
        public const float LandingPulseGlowSettleDuration = 0.24f;
        public const float LandingPulsePeakGlowAlpha = 0.44f;
        public const float LandingPulseSettleGlowAlpha = 0.18f;
        public const float GlowPulsePeakAlpha = 0.22f;
        public const float GlowPulseFadeInDuration = 0.12f;
        public const float GlowPulseScaleDuration = 0.14f;
        public const float GlowPulseSettleDuration = 0.24f;
        public const float ShimmerAlpha = 0.46f;
        public const float ShimmerFadeInDuration = 0.08f;
        public const float ShimmerFadeOutDuration = 0.12f;
        public const float MinimumShimmerTravel = 160f;
        public const float ShimmerTravelWidthMultiplier = 1.20f;

        public static readonly Vector3 EntranceStartScale = new Vector3(0.86f, 0.86f, 1f);
        public static readonly Vector3 EntranceStartRotation = new Vector3(0f, -4f, 0f);
        public static readonly Vector3 IconEntranceStartScale = new Vector3(0.92f, 0.92f, 1f);
        public static readonly Vector3 LandingPulseScale = new Vector3(1.14f, 1.14f, 1f);
        public static readonly Vector3 GlowPulseScale = new Vector3(1.10f, 1.10f, 1f);
        public static readonly Vector3 ShimmerRotation = new Vector3(0f, 0f, -12f);
        public static readonly Color LostCardColor = new Color(0.17f, 0.075f, 0.058f, 0.70f);
        public static readonly Color LostGlowColor = new Color(1f, 0.22f, 0.06f, 0.16f);

        public static float EntranceDelay(int displayIndex)
        {
            return Mathf.Min(MaxEntranceDelay, displayIndex * EntranceDelayStep);
        }

        public static Vector2 ShimmerStartOffset(float travel)
        {
            return new Vector2(-travel, -8f);
        }

        public static Vector2 ShimmerEndOffset(float travel)
        {
            return new Vector2(travel, 10f);
        }
    }
}
