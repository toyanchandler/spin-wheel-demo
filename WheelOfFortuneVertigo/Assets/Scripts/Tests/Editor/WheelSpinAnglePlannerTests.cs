using NUnit.Framework;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelSpinAnglePlannerTests
    {
        private WheelTestObjectScope _scope;

        [TearDown]
        public void TearDown()
        {
            _scope?.DestroyAll();
        }

        [Test]
        public void PlanSpin_TargetAngle_IsAheadOfLaunchByRequiredRotation()
        {
            _scope = new WheelTestObjectScope();
            WheelGameSettings settings = WheelTestFixtures.CreateSettings(_scope);
            var pointerAngles = new[] { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };

            WheelSpinPlan plan = WheelSpinAnglePlanner.PlanSpin(
                selectedIndex: 2,
                sliceCount: 8,
                startAngle: 12f,
                pointerAngles,
                settings,
                suspenseSlotCount: 24);

            float slotAngle = 360f / 8f;
            float requiredDelta = WheelSpinTiming.MinimumFastRotationBeforeSuspenseDegrees + slotAngle * 24f;

            Assert.GreaterOrEqual(plan.TargetAngle - plan.LaunchAngle, requiredDelta - 0.01f);
            Assert.Greater(plan.FastSpinDuration, 0f);
        }

        [Test]
        public void ResolveNearestSliceIndex_ReturnsClosestPointerAngle()
        {
            var pointerAngles = new[] { 10f, 100f, 190f };

            Assert.AreEqual(1, WheelSpinAnglePlanner.ResolveNearestSliceIndex(95f, pointerAngles));
        }
    }
}
