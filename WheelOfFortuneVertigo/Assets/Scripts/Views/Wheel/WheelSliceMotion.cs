using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelSliceMotion
    {
        public const float IconBoundsScale = 0.86f;

        public const float AmountBadgeAlpha = 0.92f;
        public const float AmountBadgeBackgroundAlpha = 0.96f;
        public const float AmountBadgeRimAlpha = 0.88f;

        public const float BombFirstScale = 1.18f;
        public const float BombFirstDuration = 0.11f;
        public const float BombDipScale = 0.98f;
        public const float BombDipDuration = 0.10f;
        public const float BombSecondScale = 1.12f;
        public const float BombSecondDuration = 0.10f;
        public const float BombSettleDuration = 0.16f;

        public const float RewardFirstScale = 1.18f;
        public const float RewardFirstDuration = 0.12f;
        public const float RewardDipScale = 0.96f;
        public const float RewardDipDuration = 0.09f;
        public const float RewardSecondScale = 1.12f;
        public const float RewardSecondDuration = 0.11f;
        public const float RewardSettleDuration = 0.18f;

        public static Vector3 UniformScale(float value) => new Vector3(value, value, 1f);
    }
}
