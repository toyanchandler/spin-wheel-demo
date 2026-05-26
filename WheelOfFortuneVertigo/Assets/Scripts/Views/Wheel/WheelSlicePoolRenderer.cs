using System;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Views
{
    internal static class WheelSlicePoolRenderer
    {
        public static void Render(
            WheelSliceView[] pool,
            WheelSliceDefinition[] slices,
            int sliceCount)
        {
            if (sliceCount > pool.Length)
            {
                throw new InvalidOperationException("Wheel slice pool is smaller than configured slice count. Rebuild the scene hierarchy.");
            }

            int visibleCount = Mathf.Min(sliceCount, pool.Length);
            for (int i = 0; i < visibleCount; i++)
            {
                WheelSlicePresentation presentation = new WheelSlicePresentation(
                    slices[i].DisplayIcon,
                    slices[i].DisplayAmount,
                    slices[i].DisplayColor,
                    slices[i].ShowAmountLabel,
                    slices[i].DisplayLabel);
                pool[i].Apply(presentation);
            }

            for (int i = visibleCount; i < pool.Length; i++)
            {
                pool[i].Hide();
            }
        }
    }
}
