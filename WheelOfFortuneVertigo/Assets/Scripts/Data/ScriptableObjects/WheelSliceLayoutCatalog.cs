using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [CreateAssetMenu(fileName = "WheelSliceLayoutCatalog", menuName = "Vertigo/Wheel Slice Layout Catalog")]
    public sealed class WheelSliceLayoutCatalog : ScriptableObject
    {
        [SerializeField, Min(4)] private int _presetSliceCount = 8;
        [SerializeField] private Vector2[] _normalizedPresetCenters = CreateDefaultPresetCenters();

        public int PresetSliceCount { get { return _presetSliceCount; } }
        public Vector2[] NormalizedPresetCenters { get { return _normalizedPresetCenters; } }

        public void ResetToDefaults()
        {
            _presetSliceCount = 8;
            _normalizedPresetCenters = CreateDefaultPresetCenters();
        }

        public void ValidateRuntime()
        {
            if (_normalizedPresetCenters == null || _normalizedPresetCenters.Length < _presetSliceCount)
            {
                throw new InvalidOperationException("WheelSliceLayoutCatalog requires preset centers for every preset slice.");
            }
        }

        private static Vector2[] CreateDefaultPresetCenters()
        {
            return new[]
            {
                new Vector2(-0.0042f, 0.2952f),
                new Vector2(-0.2105f, 0.2083f),
                new Vector2(-0.3007f, 0.0028f),
                new Vector2(-0.2147f, -0.2085f),
                new Vector2(-0.0001f, -0.2949f),
                new Vector2(0.2123f, -0.2051f),
                new Vector2(0.2930f, 0.0065f),
                new Vector2(0.2023f, 0.2150f)
            };
        }
    }
}
