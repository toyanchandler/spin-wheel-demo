using System;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>Subscribe to gameplay snapshot events. Views should depend on this, not snapshot publishers.</summary>
    public interface IWheelSnapshotSubscriber
    {
        event Action<WheelZoneSnapshot> ZoneChanged;
        event Action<WheelHudSnapshot> HudStateChanged;
        event Action<WheelSpinResult> SpinLanded;
        event Action<WheelOutcomeSnapshot> OutcomeResolved;
    }
}
