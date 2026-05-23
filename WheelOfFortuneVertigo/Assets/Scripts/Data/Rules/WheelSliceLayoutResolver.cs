using UnityEngine;

namespace Vertigo.Wheel.Data
{
    internal static class WheelSliceLayoutResolver
    {
        private static readonly System.Action<Vector2[], WheelSliceLayoutCatalog, float, float, int>[] FillActions =
        {
            FillPolar,
            FillPresetCenters
        };

        public static Vector2[] BuildPositions(WheelSliceLayoutCatalog catalog, WheelLayoutSettings layout, int sliceCount)
        {
            catalog.ValidateRuntime();
            float wheelSize = Mathf.Min(layout.WheelSize.x, layout.WheelSize.y);
            float radius = wheelSize * layout.SliceIconRadius;
            Vector2[] positions = new Vector2[sliceCount];
            int layoutIndex = System.Convert.ToInt32(sliceCount == catalog.PresetSliceCount);
            FillActions[layoutIndex](positions, catalog, wheelSize, radius, sliceCount);
            return positions;
        }

        public static float ResolvePointerAngle(Vector2 slicePosition)
        {
            return Mathf.Atan2(slicePosition.x, slicePosition.y) * Mathf.Rad2Deg;
        }

        private static void FillPresetCenters(Vector2[] buffer, WheelSliceLayoutCatalog catalog, float wheelSize, float radius, int sliceCount)
        {
            Vector2[] centers = catalog.NormalizedPresetCenters;
            for (int i = 0; i < sliceCount; i++)
            {
                buffer[i] = centers[i] * wheelSize;
            }
        }

        private static void FillPolar(Vector2[] buffer, WheelSliceLayoutCatalog catalog, float wheelSize, float radius, int sliceCount)
        {
            float sliceAngle = 360f / sliceCount;
            for (int i = 0; i < sliceCount; i++)
            {
                float angle = -sliceAngle * i * Mathf.Deg2Rad;
                buffer[i] = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * radius;
            }
        }
    }
}
