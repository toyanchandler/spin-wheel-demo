using System;

namespace Vertigo.Wheel.Runtime
{
    public static class WheelSpinOutcomeSelector
    {
        public static int SelectSliceIndex(int sliceCount, IRandomSource randomSource)
        {
            if (sliceCount <= 0)
            {
                throw new InvalidOperationException("Cannot select a spin slice when slice count is zero.");
            }

            if (randomSource == null)
            {
                throw new ArgumentNullException(nameof(randomSource));
            }

            return randomSource.Range(0, sliceCount);
        }
    }
}
