using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    public readonly struct WheelZoneProgressCellPresentation
    {
        public readonly bool IsActive;
        public readonly string ZoneText;
        public readonly bool ShowFrame;
        public readonly Color FrameColor;

        public WheelZoneProgressCellPresentation(bool isActive, string zoneText, bool showFrame, Color frameColor)
        {
            IsActive = isActive;
            ZoneText = zoneText;
            ShowFrame = showFrame;
            FrameColor = frameColor;
        }

        public static WheelZoneProgressCellPresentation Inactive => new WheelZoneProgressCellPresentation(false, string.Empty, false, Color.clear);
    }
}
