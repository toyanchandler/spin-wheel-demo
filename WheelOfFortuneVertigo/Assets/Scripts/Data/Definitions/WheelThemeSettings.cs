using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public sealed class WheelThemeSettings
    {
        [SerializeField] private Color _backgroundColor = new Color(0.045f, 0.05f, 0.065f, 1f);
        [SerializeField] private Color _primaryTextColor = Color.white;
        [SerializeField] private Color _secondaryTextColor = new Color(0.76f, 0.80f, 0.88f, 1f);
        [SerializeField] private Color _standardZoneColor = new Color(0.78f, 0.48f, 0.18f, 1f);
        [SerializeField] private Color _safeZoneColor = new Color(0.44f, 0.88f, 1f, 1f);
        [SerializeField] private Color _superZoneColor = new Color(0.58f, 1f, 0.18f, 1f);
        [SerializeField] private Color _safeMilestoneBadgeBackground = new Color(0.18f, 0.72f, 0.94f, 0.88f);
        [SerializeField] private Color _superMilestoneBadgeBackground = new Color(0.18f, 0.58f, 0.02f, 0.9f);
        [SerializeField] private Color _dangerColor = new Color(0.9f, 0.22f, 0.16f, 1f);
        [SerializeField] private Color _successColor = new Color(0.28f, 0.84f, 0.48f, 1f);

        public Color BackgroundColor { get { return _backgroundColor; } }
        public Color PrimaryTextColor { get { return _primaryTextColor; } }
        public Color SecondaryTextColor { get { return _secondaryTextColor; } }
        public Color StandardZoneColor { get { return _standardZoneColor; } }
        public Color SafeZoneColor { get { return _safeZoneColor; } }
        public Color SuperZoneColor { get { return _superZoneColor; } }
        public Color SafeMilestoneBadgeBackground { get { return _safeMilestoneBadgeBackground; } }
        public Color SuperMilestoneBadgeBackground { get { return _superMilestoneBadgeBackground; } }
        public Color DangerColor { get { return _dangerColor; } }
        public Color SuccessColor { get { return _successColor; } }

        public Color GetUiColor(WheelUiColorKey colorKey)
        {
            switch (colorKey)
            {
                case WheelUiColorKey.PrimaryText:
                    return _primaryTextColor;
                case WheelUiColorKey.SecondaryText:
                    return _secondaryTextColor;
                case WheelUiColorKey.StandardZone:
                    return _standardZoneColor;
                case WheelUiColorKey.SafeZone:
                    return _safeZoneColor;
                case WheelUiColorKey.SuperZone:
                    return _superZoneColor;
                case WheelUiColorKey.Danger:
                    return _dangerColor;
                case WheelUiColorKey.Success:
                    return _successColor;
                default:
                    return _primaryTextColor;
            }
        }

        public static WheelThemeSettings Default()
        {
            return new WheelThemeSettings();
        }
    }
}
