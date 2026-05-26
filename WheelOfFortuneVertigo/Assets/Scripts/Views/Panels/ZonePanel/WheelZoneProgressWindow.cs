using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public readonly struct WheelZoneProgressWindow
    {
        public WheelHudSnapshot Snapshot { get; }
        public int WindowStart { get; }

        public WheelZoneProgressWindow(WheelHudSnapshot snapshot, int cellCount)
        {
            Snapshot = snapshot;
            WindowStart = snapshot.Zone - cellCount / 2;
        }

        public int ZoneAtSlot(int slotIndex)
        {
            return WindowStart + slotIndex;
        }
    }
}
