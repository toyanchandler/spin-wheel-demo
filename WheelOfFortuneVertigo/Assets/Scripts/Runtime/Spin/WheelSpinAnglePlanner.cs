using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>Pure spin angle math. No Unity objects — safe to unit test.</summary>
    public static class WheelSpinAnglePlanner
    {
        /// <summary>Computes start, suspense, and final wheel angles for a spin.</summary>
        public static WheelSpinPlan PlanSpin(
            int selectedIndex,
            int sliceCount,
            float startAngle,
            float[] pointerAngles,
            WheelGameSettings settings,
            int suspenseSlotCount)
        {
            float launchAngle = startAngle + WheelSpinTiming.AnticipationLaunchKickDegrees;
            float slotAngle = 360f / sliceCount;
            float suspenseDegrees = slotAngle * suspenseSlotCount;
            float targetAngle = ResolveFinalTargetAngle(
                selectedIndex,
                startAngle,
                launchAngle,
                suspenseDegrees,
                pointerAngles,
                settings);
            float suspenseStartAngle = targetAngle - suspenseDegrees;
            float fastSpinDuration = ResolveFastSpinDuration(settings);

            return new WheelSpinPlan(
                startAngle,
                launchAngle,
                suspenseStartAngle,
                targetAngle,
                fastSpinDuration,
                WheelSpinTiming.SuspenseDuration,
                suspenseSlotCount);
        }

        public static int ResolveSuspenseSlotCount(int sliceCount, int randomExtraSlots)
        {
            int minimumBySliceCount = sliceCount * 2 + 8;
            int minimum = Mathf.Max(WheelSpinTiming.MinimumSuspenseSlots, minimumBySliceCount);
            return minimum + randomExtraSlots;
        }

        public static int ResolveNearestSliceIndex(float wheelAngle, float[] pointerAngles)
        {
            if (pointerAngles == null || pointerAngles.Length == 0) return -1;
            float normalizedWheelAngle = NormalizeAngle(wheelAngle);
            int nearestIndex = 0;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < pointerAngles.Length; i++)
            {
                float distance = Mathf.Abs(Mathf.DeltaAngle(normalizedWheelAngle, pointerAngles[i]));
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

        public static float EvaluateSuspenseProgress(float normalizedTime)
        {
            float clampedTime = Mathf.Clamp01(normalizedTime);
            float inverse = 1f - clampedTime;
            return 1f - Mathf.Pow(inverse, WheelSpinTiming.SuspenseEasePower);
        }

        public static float NormalizeAngle(float angle) => Mathf.Repeat(angle, 360f);

        private static float ResolveFastSpinDuration(WheelGameSettings settings)
        {
            float configuredDuration = Mathf.Max(0f, settings.SpinDuration);

            float anticipationDuration =
                WheelSpinTiming.AnticipationDelay
                + WheelSpinTiming.AnticipationPullDuration
                + WheelSpinTiming.AnticipationChargeDuration
                + WheelSpinTiming.AnticipationReleaseDuration;

            float remainingDuration = configuredDuration - anticipationDuration - WheelSpinTiming.SuspenseDuration;
            return Mathf.Max(WheelSpinTiming.MinimumFastSpinDuration, remainingDuration);
        }

        private static float ResolveFinalTargetAngle(
            int selectedIndex,
            float startAngle,
            float launchAngle,
            float suspenseDegrees,
            float[] pointerAngles,
            WheelGameSettings settings)
        {
            float selectedPointerAngle = pointerAngles[selectedIndex];

            float targetDelta =
                360f * Mathf.Max(1, settings.MinimumSpinRounds)
                + Mathf.DeltaAngle(NormalizeAngle(startAngle), NormalizeAngle(selectedPointerAngle));

            float targetAngle = startAngle + targetDelta;
            float requiredDeltaFromLaunch =
                WheelSpinTiming.MinimumFastRotationBeforeSuspenseDegrees
                + suspenseDegrees;

            while (targetAngle - launchAngle < requiredDeltaFromLaunch)
            {
                targetAngle += 360f;
            }

            return targetAngle;
        }
    }
}
