using NUnit.Framework;
using Vertigo.Wheel.Views;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelIndicatorSpinTickSimulatorTests
    {
        [Test]
        public void UpdateWhileSpinning_FirstSample_DoesNotTick()
        {
            var state = new WheelIndicatorSpinTickState();

            WheelIndicatorSpinTickResult result = WheelIndicatorSpinTickSimulator.UpdateWhileSpinning(0f, 8, ref state);

            Assert.IsFalse(result.DidTick);
            Assert.AreEqual(0f, result.OffsetDegrees);
            Assert.IsTrue(state.HasSample);
        }

        [Test]
        public void UpdateWhileSpinning_CrossingSliceBoundary_AppliesTickOffset()
        {
            var state = new WheelIndicatorSpinTickState
            {
                HasSample = true,
                LastWheelAngle = 0f,
                LastTickIndex = 0
            };

            WheelIndicatorSpinTickResult result = WheelIndicatorSpinTickSimulator.UpdateWhileSpinning(50f, 8, ref state);

            Assert.IsTrue(result.DidTick);
            Assert.AreEqual(WheelIndicatorSpinTickSimulator.TickAngleDegrees, result.OffsetDegrees);
        }

        [Test]
        public void UpdateWhileSpinning_SameSlice_OnlyTracksAngle()
        {
            var state = new WheelIndicatorSpinTickState
            {
                HasSample = true,
                LastWheelAngle = 10f,
                LastTickIndex = 0
            };

            WheelIndicatorSpinTickResult result = WheelIndicatorSpinTickSimulator.UpdateWhileSpinning(12f, 8, ref state);

            Assert.IsFalse(result.DidTick);
            Assert.AreEqual(12f, state.LastWheelAngle);
        }
    }
}
