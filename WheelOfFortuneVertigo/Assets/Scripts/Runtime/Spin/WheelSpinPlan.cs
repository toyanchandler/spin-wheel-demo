namespace Vertigo.Wheel.Runtime
{
    public readonly struct WheelSpinPlan
    {
        public readonly float StartAngle;
        public readonly float LaunchAngle;
        public readonly float SuspenseStartAngle;
        public readonly float TargetAngle;
        public readonly float FastSpinDuration;
        public readonly float SuspenseDuration;
        public readonly int SuspenseSlotCount;

        public WheelSpinPlan(
            float startAngle,
            float launchAngle,
            float suspenseStartAngle,
            float targetAngle,
            float fastSpinDuration,
            float suspenseDuration,
            int suspenseSlotCount)
        {
            StartAngle = startAngle;
            LaunchAngle = launchAngle;
            SuspenseStartAngle = suspenseStartAngle;
            TargetAngle = targetAngle;
            FastSpinDuration = fastSpinDuration;
            SuspenseDuration = suspenseDuration;
            SuspenseSlotCount = suspenseSlotCount;
        }
    }
}
