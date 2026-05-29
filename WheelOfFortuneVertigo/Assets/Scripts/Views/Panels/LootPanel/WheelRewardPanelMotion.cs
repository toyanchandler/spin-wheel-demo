using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelRewardPanelMotion
    {
        public const float ScrollSettleDuration = 0.18f;
        public const float ScrollOverflowTolerance = 1f;
        public const float ScrollPositionTolerance = 0.001f;
        public const float ScrollTopNormalizedPosition = 1f;
        public const float PendingFallbackCommitDelay = 2.35f;
        public const float LandingPulseDelayStep = 0.02f;
        public const float LandingSpreadMaximum = 26f;
        public const float LandingSpreadBase = 8f;
        public const float LandingSpreadPerBurst = 2f;
        public const float LandingVerticalJitter = 8f;

        public static float LandingPulseDelay(int cardIndex) => LandingPulseDelayStep * cardIndex;

        public static Vector3 LandingSpreadOffset(int burstIndex, int burstCount)
        {
            if (burstCount <= 1) return Vector3.zero;
            float spread = Mathf.Min(LandingSpreadMaximum, LandingSpreadBase + (burstCount * LandingSpreadPerBurst));
            float offsetX = Mathf.Lerp(-spread, spread, burstIndex / Mathf.Max(1f, burstCount - 1f));
            float offsetY = Random.Range(-LandingVerticalJitter, LandingVerticalJitter);
            return new Vector3(offsetX, offsetY, 0f);
        }
    }
}
