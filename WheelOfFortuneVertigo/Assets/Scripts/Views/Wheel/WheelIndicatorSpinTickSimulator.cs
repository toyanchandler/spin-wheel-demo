using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal struct WheelIndicatorSpinTickState
    {
        public float LastWheelAngle;
        public int LastTickIndex;
        public bool HasSample;

        public void Seed(float wheelAngle, int tickIndex)
        {
            LastWheelAngle = wheelAngle;
            LastTickIndex = tickIndex;
            HasSample = true;
        }

        public void TrackAngle(float wheelAngle)
        {
            LastWheelAngle = wheelAngle;
        }

        public void TrackBoundaryCrossing(float wheelAngle, int tickIndex)
        {
            LastWheelAngle = wheelAngle;
            LastTickIndex = tickIndex;
        }

        public void ClearSample()
        {
            HasSample = false;
        }
    }

    internal readonly struct WheelIndicatorSpinTickResult
    {
        public bool DidTick { get; }
        public float OffsetDegrees { get; }

        public static WheelIndicatorSpinTickResult None => new WheelIndicatorSpinTickResult(false, 0f);

        public static WheelIndicatorSpinTickResult Tick(float offsetDegrees)
        {
            return new WheelIndicatorSpinTickResult(true, offsetDegrees);
        }

        private WheelIndicatorSpinTickResult(bool didTick, float offsetDegrees)
        {
            DidTick = didTick;
            OffsetDegrees = offsetDegrees;
        }
    }

    /// <summary>Pure tick math for indicator motion during spin — unit-testable without MonoBehaviour.</summary>
    internal static class WheelIndicatorSpinTickSimulator
    {
        public const float TickAngleDegrees = 18f;
        public const float ReturnSpeedDegreesPerSecond = 420f;

        public static int ResolveTickIndex(float wheelAngleDegrees, int sliceCount)
        {
            int safeSliceCount = Mathf.Max(1, sliceCount);
            float degreesPerSlice = 360f / safeSliceCount;
            return Mathf.FloorToInt(wheelAngleDegrees / degreesPerSlice);
        }

        public static float AdvanceReturnTowardZero(float currentOffset, float deltaTime)
        {
            float step = ReturnSpeedDegreesPerSecond * deltaTime;
            return Mathf.MoveTowards(currentOffset, 0f, step);
        }

        public static float ResolveSpinDirection(float lastWheelAngle, float wheelAngle)
        {
            float spinDirection = Mathf.Sign(Mathf.DeltaAngle(lastWheelAngle, wheelAngle));
            return Mathf.Approximately(spinDirection, 0f) ? 1f : spinDirection;
        }

        public static WheelIndicatorSpinTickResult UpdateWhileSpinning(
            float wheelAngle,
            int sliceCount,
            ref WheelIndicatorSpinTickState state)
        {
            int tickIndex = ResolveTickIndex(wheelAngle, sliceCount);

            if (!state.HasSample)
            {
                state.Seed(wheelAngle, tickIndex);
                return WheelIndicatorSpinTickResult.None;
            }

            if (tickIndex == state.LastTickIndex)
            {
                state.TrackAngle(wheelAngle);
                return WheelIndicatorSpinTickResult.None;
            }

            float offset = ResolveBoundaryTickOffset(state.LastWheelAngle, wheelAngle);
            state.TrackBoundaryCrossing(wheelAngle, tickIndex);
            return WheelIndicatorSpinTickResult.Tick(offset);
        }

        private static float ResolveBoundaryTickOffset(float lastWheelAngle, float wheelAngle)
        {
            float direction = ResolveSpinDirection(lastWheelAngle, wheelAngle);
            return direction * TickAngleDegrees;
        }
    }
}
