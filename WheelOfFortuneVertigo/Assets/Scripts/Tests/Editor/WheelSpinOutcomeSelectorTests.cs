using NUnit.Framework;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelSpinOutcomeSelectorTests
    {
        [Test]
        public void SelectSliceIndex_UsesDeterministicRandomSource()
        {
            var randomA = new SeededRandomSource(42);
            var randomB = new SeededRandomSource(42);

            int first = WheelSpinOutcomeSelector.SelectSliceIndex(8, randomA);
            int second = WheelSpinOutcomeSelector.SelectSliceIndex(8, randomB);

            Assert.AreEqual(first, second);
            Assert.GreaterOrEqual(first, 0);
            Assert.Less(first, 8);
        }

        [Test]
        public void SelectSliceIndex_ReturnsDifferentValues_WhenRandomAdvances()
        {
            var random = new SeededRandomSource(1);

            int first = WheelSpinOutcomeSelector.SelectSliceIndex(8, random);
            int second = WheelSpinOutcomeSelector.SelectSliceIndex(8, random);

            Assert.AreNotEqual(first, second);
        }
    }
}
