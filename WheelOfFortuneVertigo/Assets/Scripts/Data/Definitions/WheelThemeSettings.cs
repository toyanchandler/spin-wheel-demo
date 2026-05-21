using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public sealed class WheelThemeSettings
    {
        public Color backgroundColor = new Color(0.045f, 0.05f, 0.065f, 1f);
        public Color primaryTextColor = Color.white;
        public Color secondaryTextColor = new Color(0.76f, 0.80f, 0.88f, 1f);
        public Color standardZoneColor = new Color(0.78f, 0.48f, 0.18f, 1f);
        public Color safeZoneColor = new Color(0.68f, 0.76f, 0.88f, 1f);
        public Color superZoneColor = new Color(1f, 0.78f, 0.22f, 1f);
        public Color dangerColor = new Color(0.9f, 0.22f, 0.16f, 1f);
        public Color successColor = new Color(0.28f, 0.84f, 0.48f, 1f);

        [SerializeField] private Color[] _uiColorPalette = CreateDefaultPalette();

        public Color GetUiColor(WheelUiColorKey colorKey)
        {
            return _uiColorPalette[(int)colorKey];
        }

        public void SyncUiColorPalette()
        {
            _uiColorPalette = CreateDefaultPalette();
        }

        public static WheelThemeSettings Default()
        {
            var theme = new WheelThemeSettings();
            theme.SyncUiColorPalette();
            return theme;
        }

        private static Color[] CreateDefaultPalette()
        {
            return new[]
            {
                Color.white,
                new Color(0.76f, 0.80f, 0.88f, 1f),
                new Color(0.78f, 0.48f, 0.18f, 1f),
                new Color(0.68f, 0.76f, 0.88f, 1f),
                new Color(1f, 0.78f, 0.22f, 1f),
                new Color(0.9f, 0.22f, 0.16f, 1f),
                new Color(0.28f, 0.84f, 0.48f, 1f)
            };
        }
    }
}
