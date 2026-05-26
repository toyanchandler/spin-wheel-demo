using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelRewardOpeningMotion
    {
        public const float ShowFadeDuration = 0.22f;
        public const float ShowScaleDuration = 0.28f;
        public const float HideFadeDuration = 0.16f;
        public const float RevealScrollLeadIn = 0.10f;
        public const float RevealScrollSettle = 0.36f;
        public const float ScrollButtonDuration = 0.24f;
        public const float ScrollButtonStepCards = 1f;
        public const float ScrollEdgePadding = 32f;
        public const float ScrollEndTolerance = 0.01f;
        public const float MinimumScrollDuration = 0.24f;
        public const float DefaultCardWidth = 370f;
        public const float DefaultCardStep = 418f;
        public const float DefaultCardSpacing = 48f;

        public static readonly Vector3 ShowStartScale = new Vector3(0.96f, 0.96f, 1f);
    }
}
