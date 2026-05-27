using System;
using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    public static class WheelSpinOutcomeSelector
    {
        public static int SelectSliceIndex(int sliceCount)
        {
            if (sliceCount <= 0)
            {
                throw new InvalidOperationException("Cannot select a spin slice when slice count is zero.");
            }

            return UnityEngine.Random.Range(0, sliceCount);
        }
    }
}
