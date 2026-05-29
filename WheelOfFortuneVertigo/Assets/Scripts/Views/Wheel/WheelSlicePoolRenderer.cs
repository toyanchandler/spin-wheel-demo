using System;
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal static class WheelSlicePoolRenderer
    {
        public static void Render(
            WheelSliceView[] pool,
            WheelSlicePresentation[] presentations,
            int sliceCount)
        {
            if (sliceCount > pool.Length) throw new InvalidOperationException("Wheel slice pool is smaller than configured slice count. Rebuild the scene hierarchy.");
            int visibleCount = Mathf.Min(sliceCount, pool.Length);
            for (int i = 0; i < visibleCount; i++)
            {
                pool[i].Apply(presentations[i]);
            }

            for (int i = visibleCount; i < pool.Length; i++)
            {
                pool[i].Hide();
            }
        }
    }
}
