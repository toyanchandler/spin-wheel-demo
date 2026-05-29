namespace Vertigo.Wheel.Runtime
{
    public static class WheelSpinTiming
    {
        public const float AnticipationDelay = 0.03f;
        public const float AnticipationPullDuration = 0.14f;
        public const float AnticipationChargeDuration = 0.08f;
        public const float AnticipationReleaseDuration = 0.07f;
        public const float AnticipationPullDegrees = -14f;
        public const float AnticipationLaunchKickDegrees = 7f;

        public const float MinimumFastSpinDuration = 0.70f;
        public const float MinimumFastRotationBeforeSuspenseDegrees = 540f;

        public const int MinimumSuspenseSlots = 22;
        public const int ExtraRandomSuspenseSlots = 7;
        public const float SuspenseDuration = 3.90f;
        public const float SuspenseEasePower = 1.95f;

        public const float LandingPunchDuration = 0.18f;
        public const float LandingPunchScale = 0.045f;
        public const float PostLandingHoldDuration = 0.04f;
    }
}
