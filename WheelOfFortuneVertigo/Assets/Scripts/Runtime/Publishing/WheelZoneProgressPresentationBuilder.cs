using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    public static class WheelZoneProgressPresentationBuilder
    {
        public static WheelZoneProgressCellPresentation Build(WheelHudSnapshot snapshot, int windowStart, int slotIndex)
        {
            int zone = windowStart + slotIndex;
            if (zone <= 0) return WheelZoneProgressCellPresentation.Inactive;

            WheelHudMilestoneSnapshot milestones = snapshot.Milestones;
            WheelZoneProgressCellKind kind = ResolveKind(zone, milestones.SafeZoneInterval, milestones.SuperZoneInterval);
            bool isCurrentZone = zone == snapshot.Zone;
            bool showFrame = isCurrentZone || kind != WheelZoneProgressCellKind.Standard;
            Color frameColor = ResolveFrameColor(kind, isCurrentZone, milestones);

            return new WheelZoneProgressCellPresentation(
                true,
                zone.ToString(),
                showFrame,
                frameColor);
        }

        private static WheelZoneProgressCellKind ResolveKind(int zone, int safeInterval, int superInterval)
        {
            if (IsIntervalZone(zone, superInterval)) return WheelZoneProgressCellKind.Super;
            return IsIntervalZone(zone, safeInterval)
                ? WheelZoneProgressCellKind.Safe
                : WheelZoneProgressCellKind.Standard;
        }

        private static bool IsIntervalZone(int zone, int interval) => interval > 0 && zone % interval == 0;

        private static Color ResolveFrameColor(
            WheelZoneProgressCellKind kind,
            bool isCurrentZone,
            WheelHudMilestoneSnapshot milestones)
        {
            if (kind == WheelZoneProgressCellKind.Super) return milestones.SuperMilestoneBadgeColor;
            if (kind == WheelZoneProgressCellKind.Safe) return milestones.SafeMilestoneBadgeColor;
            return isCurrentZone
                ? new Color(1f, 1f, 1f, 0.92f)
                : milestones.SafeMilestoneBadgeColor;
        }
    }

    public enum WheelZoneProgressCellKind
    {
        Standard,
        Safe,
        Super
    }
}
